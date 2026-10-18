# CourseLibrary Event Consumer Project

**Document Type:** Solution Architecture Requirements  
**Project:** CourseLibrary Event Consumer 
**Technology Stack:** .NET / ASP.NET Core Web API, Azure Service Bus, Azure Durable Functions, Azure Cosmos DB, and Azure Table Storage  
**Status:** Requirements Definition and Architecture Planning

---

# 1. Purpose and Objectives

The objective is to design a reliable, scalable, and asynchronous audit event processing system for the CourseLibrary platform.

The system must capture events associated with database operations performed by the primary CourseLibrary API, publish these events asynchronously through Azure Service Bus, and process them using Azure Durable Functions. The processed audit records must be persisted in a dedicated audit container in Azure Cosmos DB.

The design must prioritize eventual consistency, controlled resource consumption, horizontal scalability, fault tolerance, and operational observability while acknowledging that minor data loss may be acceptable under explicitly defined failure conditions.

The architecture must support high API traffic without allowing audit processing to overwhelm the database or degrade the primary API's performance.

## 1.1 Primary Objectives

- Decouple audit event publication from audit persistence.
- Support asynchronous, topic-based event delivery and consumption.
- Sustain an API traffic capacity of 10,000 requests per 10 seconds.
- Support an audit processing target of up to 2,000 messages per 10 seconds, subject to database capacity.
- Prevent Azure Cosmos DB throttling from causing uncontrolled function scaling.
- Configure bounded concurrency, retries, and horizontal scaling.
- Define message retention, dead-letter handling, and failed-message recovery.
- Provide operational visibility into processing failures, backlogs, and data loss.
- Establish measurable service-level objectives for throughput, latency, and reliability.

# 2. High-Level Architecture

The proposed architecture consists of the following components:

1. **CourseLibrary API:** Executes the primary business operations and initiates audit event publication.
2. **Azure Service Bus Topic:** Buffers audit events and decouples producers from consumers.
3. **Azure Durable Functions:** Consumes audit events, coordinates processing, and persists audit records.
4. **Azure Cosmos DB — Audit Container:** Stores processed audit records separately from the primary application data.
5. **Azure Table Storage:** Stores metadata for dead-letter messages that have been captured for analysis and recovery.
6. **Dead-Letter Queue (DLQ):** Retains messages that cannot be successfully processed within the configured delivery and retry policies.
7. **Monitoring and Alerting:** Tracks throughput, latency, backlog, retries, throttling, failures, and recovery operations.

### High-Level Processing Flow

1. A client sends a request to the CourseLibrary API.
2. The API performs the relevant business operation against Azure Cosmos DB.
3. The API creates an audit event representing the relevant business activity.
4. The API publishes the event to the configured Azure Service Bus topic.
5. The Service Bus trigger receives the message and initiates the corresponding Durable Functions orchestration.
6. The system acknowledges the Service Bus message only after the orchestration has been durably scheduled and the ownership handoff is safe.
7. The orchestration coordinates audit persistence, subject to the configured concurrency and retry limits.
8. The audit record is written to the dedicated Cosmos DB audit container.
9. Failed messages are retried according to the configured policy and eventually routed to the DLQ when the delivery limit is exceeded.
10. DLQ messages are recorded in Azure Table Storage for analysis, monitoring, and controlled recovery.

The implementation must account for the possibility that the orchestration is successfully scheduled but fails later. Service Bus message completion must not be interpreted as confirmation that the audit record has been persisted.

# 3. Functional Requirements

## FR-01: Audit Event Creation

The system shall identify relevant business operations that require audit tracking.

For each applicable operation, the system shall create an audit event containing the information necessary to identify and reconstruct the recorded activity.

The event schema shall define, at a minimum:

- Event identifier.
- Event type.
- Event creation timestamp.
- Relevant entity or aggregate identifier.
- Operation or activity type.
- Actor or service identity, where applicable.
- Correlation or trace identifier.
- Required audit metadata.

The schema shall be versioned to support future evolution.

## FR-02: Asynchronous Event Publication

The CourseLibrary API shall publish audit events to an Azure Service Bus topic rather than synchronously writing audit records to the audit container.

The publication mechanism shall support transient failure handling, bounded retries, and appropriate logging.

The design shall explicitly address the consistency gap between the business database operation and event publication. Where stronger delivery guarantees are required, the architecture shall evaluate the transactional outbox pattern.

## FR-03: Topic-Based Message Distribution

Azure Service Bus shall provide topic-based message distribution.

The architecture shall define the topic, subscription, filtering rules, and delivery configuration required for audit event processing.

The design shall also establish whether additional subscriptions are needed for independent consumers, replay, or operational recovery.

## FR-04: Durable Function Consumption

Azure Durable Functions shall consume audit events through Azure Service Bus triggers and coordinate the processing workflow.

The implementation shall define:

- Trigger execution behavior.
- Orchestration scheduling and handoff.
- Message settlement and completion.
- Orchestration instance identification.
- Duplicate event detection.
- Failure handling and replay behavior.

The system shall prevent duplicate delivery from creating unintended duplicate audit records.

## FR-05: Audit Record Persistence

The consumer shall persist audit records into a dedicated Azure Cosmos DB audit container.

The audit container shall have its own throughput configuration, partition key, indexing policy, and capacity limits, independently of the primary application container where supported by the selected Cosmos DB configuration.

Audit writes shall use an idempotent persistence strategy where practical.

## FR-06: Bounded Processing Concurrency

The system shall limit concurrent processing at multiple levels:

- Number of messages prefetched and processed by each Function host.
- Number of concurrent trigger executions per instance.
- Number of active Durable Functions orchestrations.
- Number of concurrent activities within an orchestration.
- Number of simultaneous database writes.

Concurrency shall be configured to prevent the audit workload from exceeding the available database throughput.

The design shall not assume that the maximum number of running orchestrations is equivalent to the number of simultaneous database writes.

## FR-07: Retry and Failure Handling

The consumer shall support retry handling for transient errors, including temporary database unavailability, throttling, and intermittent network failures.

The retry policy shall specify:

- Maximum retry attempts.
- Initial retry delay.
- Backoff strategy and maximum delay.
- Treatment of HTTP 429 responses and `Retry-After` guidance.
- Classification of transient and permanent failures.
- Final disposition after retry exhaustion.

Cosmos DB SDK retries and application-level retries shall be coordinated to avoid excessive cumulative delays and retry amplification.

Messages that exhaust the applicable delivery policy shall be routed to the DLQ or an explicitly defined failure-processing mechanism.

## FR-08: Dead-Letter Management

The system shall retain failed messages in the Service Bus DLQ for operational investigation and recovery.

A DLQ processing component shall record relevant metadata in Azure Table Storage, including the message identifier, event type, failure reason, dead-letter timestamp, retry information, and recovery status, subject to the chosen data model.

The solution shall support:

- Inspection of dead-letter messages.
- Classification of failure causes.
- Controlled replay or resubmission.
- Prevention of duplicate audit writes during recovery.
- Cleanup of processed DLQ metadata.
- Monitoring of unprocessed and repeatedly failing messages.

Azure Table Storage shall be treated as an operational metadata store, not as a substitute for retaining the original dead-letter message.

## FR-09: Observability

The system shall provide structured logs, distributed tracing, metrics, and alerts covering the complete event lifecycle.

The implementation shall track at least:

- Events published and publication failures.
- Messages received and completed.
- Orchestrations scheduled, running, completed, and failed.
- Audit write throughput and latency.
- Cosmos DB 429 responses and retry activity.
- Active Function instances and processing concurrency.
- Service Bus active-message count and oldest-message age.
- DLQ message count and age.
- End-to-end audit processing latency.
- Events potentially lost before successful publication or persistence.

## FR-10: Failed-Message Recovery

The solution shall define an operational workflow for retrying or replaying failed events.

Recovery shall be controlled and observable. It shall preserve event identity, maintain idempotency, and avoid unintentionally bypassing database throughput controls.

Where the normal consumer cannot recover a failure, the design shall evaluate a dedicated retry topic or queue with independent delivery and retry policies.

# 4. Non-Functional Requirements

## NFR-01: API Throughput

The primary CourseLibrary API shall support a configured traffic capacity of **10,000 requests per 10 seconds**, equivalent to an average of 1,000 requests per second over the measurement interval.

This is a capacity target, not yet a confirmed sustained-load or peak-load service-level objective.

The solution design shall establish whether this target applies to all requests or only to the relevant API routes, and shall validate the target through load testing.

## NFR-02: Audit Processing Throughput

The audit processing system shall target a maximum processing rate of **2,000 messages per 10 seconds**, equivalent to 200 messages per second.

The system shall distinguish between:

- Maximum allowed processing throughput.
- Sustainable throughput under normal database conditions.
- Burst ingestion rate.
- Recovery throughput after an outage.

The consumer shall not exceed the configured processing limit simply because additional Function instances become available.

## NFR-03: Database Throughput Protection

The audit Cosmos DB container is expected to have an initial configured capacity of approximately **1,000 RU/s**, subject to validation against actual event sizes, indexing costs, write patterns, and partition distribution.

The architecture shall ensure that the consumer's aggregate database request rate remains within the sustainable capacity of the audit container.

The system shall handle throttling gracefully rather than relying on automatic horizontal scaling alone.

## NFR-04: Horizontal Scalability

The API and Azure Functions shall scale independently according to their workload characteristics.

The design shall specify:

- Minimum and maximum API instance counts.
- Minimum and maximum Function instance counts.
- Scale-out and scale-in behavior.
- Concurrency per instance.
- Scale-out response time.
- Database-aware processing limits.
- Behavior during sustained database throttling.

Function scaling shall be bounded so that additional instances do not generate an uncontrolled increase in database requests.

## NFR-05: Latency

The solution design shall define measurable latency targets for:

- API request processing.
- Event publication.
- Time from publication to consumption.
- Time from publication to successful audit persistence.
- Recovery after a transient failure.

The latency targets shall be established through performance testing and agreed service-level objectives.

The relationship between throughput, latency, and concurrency shall be used for capacity planning:

**Concurrency ≈ Throughput × Average processing latency**

The calculation shall use consistent units and shall be supplemented with tail-latency measurements, including p95 and p99, where applicable.

## NFR-06: Eventual Consistency

Audit persistence shall be eventually consistent with the originating business operation.

The system shall define an acceptable delay between the successful business operation and the availability of its audit record.

The design shall acknowledge that an API operation can succeed while audit publication or persistence is delayed or fails.

The accepted degree of potential data loss shall be documented explicitly and validated against the selected publication mechanism.

## NFR-07: Delivery Reliability and Idempotency

The architecture shall define the expected delivery semantics and protect against duplicate messages.

The consumer shall use a stable event identifier and an idempotent persistence mechanism so that retries and replays do not create unintended duplicate audit records.

The solution shall not assume that Service Bus delivery, Durable Functions scheduling, orchestration execution, and Cosmos DB writes form a single atomic transaction.

## NFR-08: Message Retention

The system shall target the following retention periods:

- Active Service Bus messages: up to 15 days.
- Dead-letter messages: up to 30 days.

The design shall validate the actual Service Bus tier and entity configuration against these requirements. Where a 30-day DLQ retention period cannot be configured directly, an alternative archival mechanism shall be specified.

The system shall define what happens when messages reach the end of their retention period without successful processing.

## NFR-09: Resource Efficiency and Cost

The architecture shall balance throughput, latency, availability, and operating cost.

The design shall evaluate the cost implications of:

- API and Function instance scaling.
- Service Bus tier and message operations.
- Cosmos DB provisioned or autoscale throughput.
- Durable Functions orchestration history.
- Table Storage operations.
- Monitoring and logging retention.

## NFR-10: Failure Isolation and Recovery

The system shall isolate audit processing failures from the primary API wherever practical.

It shall provide bounded retries, backpressure, controlled replay, and operational alerts.

The architecture shall explicitly define behavior during extended Cosmos DB outages, Service Bus unavailability, and Function deployment or scaling events.

# 5. Capacity Planning and Scaling Requirements

## 5.1 API Capacity

The API shall be designed around a target of 10,000 requests per 10 seconds.

The architecture review shall determine the required number of API instances using measured request latency, CPU and memory utilization, database consumption, and the actual platform scaling limits.

The capacity plan shall consider whether every API request generates an audit event or only a subset of business operations.

## 5.2 Audit Consumer Capacity

The consumer shall target up to 2,000 messages per 10 seconds, subject to the available audit-container throughput.

The design shall determine the appropriate values for:

- Maximum concurrent message processing per Function instance.
- Maximum number of Function instances.
- Maximum active orchestrations.
- Maximum concurrent audit writes.
- Message prefetch and batch size.
- Maximum outstanding work during database throttling.

## 5.3 Cosmos DB Capacity

The audit container shall initially be evaluated against a throughput budget of approximately 1,000 RU/s.

Capacity planning shall measure the RU cost of an individual audit write, including indexing overhead and any additional reads or transactional operations.

The design shall calculate sustainable write throughput as a function of actual RU consumption and the usable RU/s budget.

Autoscale or other supported throughput management shall be evaluated against the expected workload, while respecting partition-level limits and the platform's maximum configured throughput.

## 5.4 Database-Aware Backpressure

If the audit container is unavailable or repeatedly returns throttling responses, the consumer shall reduce or pause database-bound work instead of continuing to scale out without limit.

Service Bus shall serve as the durable buffer within its configured capacity and retention constraints.

The design shall specify how processing resumes when the database recovers and how backlog-drain rates are controlled.

# 6. Message Retry and Delivery Requirements

The solution shall define distinct retry policies for:

1. Service Bus message delivery.
2. Azure Cosmos DB SDK operations.
3. Durable Functions orchestration and activity execution.
4. Application-level retries.
5. Operational replay of dead-letter messages.

The retry design shall prevent excessive cumulative delays and duplicate side effects.

The design shall also define the relationship between Service Bus delivery counts and failures occurring after an orchestration has already been scheduled.

A successfully scheduled orchestration shall not be treated as successfully completed audit processing. Durable Functions must provide the appropriate persistence and recovery guarantees for work accepted after the Service Bus message has been settled.

# 7. Assumptions

The following assumptions apply unless revised during architecture review.

**ASM-01:** The primary application is implemented using ASP.NET Core Web API on .NET.

**ASM-02:** Azure Service Bus topics and subscriptions are the primary transport mechanism for audit events.

**ASM-03:** Azure Durable Functions coordinate audit event processing and persistence.

**ASM-04:** Azure Cosmos DB is the audit record store, using a container separate from the primary application data container.

**ASM-05:** Azure Table Storage is used to retain DLQ processing metadata for investigation and recovery.

**ASM-06:** Audit processing is asynchronous. The API is not required to wait for the audit record to be persisted before responding, except where a future business requirement specifies otherwise.

**ASM-07:** The target API capacity is 10,000 requests per 10 seconds, and the target audit processing capacity is 2,000 messages per 10 seconds.

**ASM-08:** The audit container initially has an estimated capacity of 1,000 RU/s. This is a planning assumption and must be validated using measured RU consumption.

**ASM-09:** Some audit processing delay is acceptable, and the business can tolerate a limited amount of data loss under explicitly agreed circumstances.

**ASM-10:** The architecture must support duplicate detection, idempotent processing, retry handling, and controlled recovery.

**ASM-11:** The API and consumer may scale independently, but consumer scaling must remain constrained by the database's sustainable processing capacity.

# 8. Prerequisites

Before finalizing the solution architecture, the following information and decisions must be available.

**PRE-01: Audit event definition**

Define which business operations generate audit events, the required event schema, event versioning strategy, and mandatory audit fields.

**PRE-02: Data-loss tolerance**

Define the maximum acceptable audit event loss, if any, and the failure scenarios in which loss is considered acceptable.

**PRE-03: Consistency target**

Define the maximum acceptable delay between a successful business operation and the corresponding persisted audit record.

**PRE-04: Workload profile**

Clarify whether 10,000 requests per 10 seconds represents sustained traffic, burst traffic, or a rate-limiting ceiling. Establish the expected proportion of requests that produce audit events.

**PRE-05: Database measurements**

Measure the RU consumption of representative audit writes, expected event size, indexing overhead, and partition-key distribution.

**PRE-06: Hosting and scaling model**

Confirm the Azure hosting plans, Functions runtime model, Durable Functions storage provider, Service Bus tier, and applicable scaling limits.

**PRE-07: Retention validation**

Confirm whether the selected Service Bus configuration supports the required message and DLQ retention periods. Identify any archival or scheduled-cleanup requirements.

**PRE-08: Recovery ownership**

Identify the operational owner responsible for investigating DLQ messages, approving replay, and handling permanently failed events.

**PRE-09: Observability**

Define logging, tracing, metrics, alert thresholds, and the monitoring platform.

**PRE-10: Service-level objectives**

Agree the target API latency, audit processing latency, availability, backlog recovery time, and maximum tolerated failure duration.

# 9. Constraints

**CON-01:** The primary API has a configured traffic limit of 10,000 requests per 10 seconds.

**CON-02:** Audit processing must be bounded to a target of 2,000 messages per 10 seconds and may be lower when database capacity is constrained.

**CON-03:** The audit Cosmos DB container is initially assumed to have 1,000 RU/s of configured capacity.

**CON-04:** Function horizontal scaling must not cause uncontrolled Cosmos DB request growth or persistent 429 throttling.

**CON-05:** The system must operate asynchronously and tolerate delayed audit persistence.

**CON-06:** The Service Bus message-retention and DLQ-retention requirements must be reconciled with the selected service tier and supported configuration.

**CON-07:** Retrying, rescheduling, or replaying an event must not cause unintended duplicate audit records.

**CON-08:** Completing a Service Bus message after orchestration scheduling must not compromise the durability or recoverability of the scheduled processing.

**CON-09:** The system must account for transient and permanent failures across the API, Service Bus, Durable Functions, and Cosmos DB.

# 10. Out of Scope

The following capabilities are excluded from this architecture scope unless explicitly added later.

**OOS-01: Authentication and Authorization**

User authentication and authorization are handled at the API gateway level using a separate identity provider (IDP). Designing or implementing the IDP, user authentication flows, and authorization policies is outside this scope.

**OOS-02: API Gateway Implementation**

The internal design and implementation of the API gateway, including its routing and identity-provider integration, are excluded.

**OOS-03: User Interface**

No administrative dashboard or user-facing interface is included, except for the operational capabilities explicitly required to support DLQ investigation and recovery.

**OOS-04: General-Purpose Audit Analytics**

Business intelligence, reporting, advanced analytics, and long-term analytical warehousing are excluded.

**OOS-05: General Event Platform**

The design is focused on audit events and the associated processing workflow. A general-purpose enterprise event platform is excluded unless required to meet the audit system's stated needs.

# 11. Expected Architecture Deliverables

The solution architecture phase shall produce the following deliverables:

1. High-level architecture diagram and component responsibilities.
2. End-to-end event lifecycle and sequence diagrams.
3. Event schema and versioning strategy.
4. Service Bus topic, subscription, retention, and DLQ configuration.
5. Durable Functions orchestration and activity design.
6. Retry, idempotency, duplicate detection, and message settlement strategy.
7. Cosmos DB RU/s capacity calculation and partitioning strategy.
8. API and Function horizontal scaling and concurrency configuration.
9. Backpressure and database outage recovery strategy.
10. DLQ metadata model, replay workflow, and cleanup process.
11. Observability, alerting, and operational runbook.
12. Performance test plan and measurable acceptance criteria.
13. Cost and capacity estimates for normal, peak, and recovery workloads.

# 12. Acceptance Criteria

The solution shall be considered ready for production approval when the following criteria have been validated.

- The API meets its agreed throughput and latency targets under representative load.
- Audit events are published and processed asynchronously.
- Duplicate delivery and retry scenarios do not create unintended duplicate audit records.
- Function concurrency and scaling remain within the defined database capacity budget.
- Cosmos DB throttling and temporary unavailability do not cause uncontrolled scaling or retry amplification.
- Failed messages follow the defined retry and DLQ policies.
- DLQ messages can be inspected, tracked, and replayed safely.
- The configured message retention meets the approved retention requirements or an approved alternative is implemented.
- Monitoring detects publication failures, excessive audit latency, growing backlog, database throttling, and DLQ accumulation.
- The accepted data-loss risk and recovery objectives are documented and approved.
- Performance, resilience, and failure-recovery tests demonstrate that the architecture meets the agreed service-level objectives.
