# CourseLibrary Event Consumer Project

**Document Type:** Solution Architecture Requirements  
**Project:** CourseLibrary Event Consumer 
**Technology Stack:** .NET / ASP.NET Core Web API, Azure Service Bus, Azure Cosmos DB, and Azure Table Storage
**Status:** Requirements Definition and Architecture Planning

---

# 1. Purpose and Objectives

The objective is to design a reliable, scalable, and asynchronous audit event processing system for the CourseLibrary platform.

The system must capture events associated with database operations performed by the primary CourseLibrary API, publish them asynchronously through Azure Service Bus, and persist audit records in a dedicated Azure Cosmos DB container. A successful Service Bus-triggered Function invocation is completed automatically; a failed invocation is left to Service Bus retry and eventually dead-letter according to the subscription's configured delivery limit.

The design must prioritize eventual consistency, controlled resource consumption, horizontal scalability, fault tolerance, and operational observability. Once Service Bus accepts an event, no processing or archival failure may silently discard it: retry it, retain it in the subscription DLQ, or keep the DLQ message unsettled until archival succeeds. The producer-side business-commit/publication gap remains an explicit outbox requirement; this consumer design alone cannot guarantee end-to-end zero loss.

The architecture must support high API traffic without allowing audit processing to overwhelm the database or degrade the primary API's performance.

## 1.1 Primary Objectives

- Decouple audit event publication from audit persistence.
- Support asynchronous, topic-based event delivery and consumption.
- Sustain an API traffic capacity of 10,000 requests per 10 seconds.
- Support a provisional sustained audit processing capacity of 600 events/second for a 10-second burst of 1,000 events/second followed by 500 events/second, subject to measured Cosmos RU cost and p95 latency validation.
- Prevent Azure Cosmos DB throttling from causing uncontrolled function scaling.
- Configure bounded concurrency, retries, and horizontal scaling.
- Define message retention, dead-letter handling, and failed-message recovery.
- Provide operational visibility into processing failures, backlogs, and data loss.
- Establish measurable service-level objectives for throughput, latency, and reliability.

# 2. High-Level Architecture

The proposed architecture consists of the following components:

1. **CourseLibrary API:** Executes the primary business operations and initiates audit event publication.
2. **Azure Service Bus Topic:** Buffers audit events and decouples producers from consumers.
3. **.NET isolated Azure Function:** Validates each message and schedules a Durable orchestration; the Service Bus binding completes the message automatically when the Function invocation returns successfully.
4. **Azure Cosmos DB — Audit Container:** Stores processed audit records separately from the primary application data.
5. **Azure Table Storage:** Stores metadata for dead-letter messages that have been captured for analysis and recovery.
6. **Dead-Letter Queue (DLQ):** Retains messages that cannot be successfully processed within the configured delivery and retry policies.
7. **Monitoring and Alerting:** Tracks throughput, latency, backlog, retries, throttling, failures, and recovery operations.

### High-Level Processing Flow

1. A client sends a request to the CourseLibrary API.
2. The API performs the relevant business operation against Azure Cosmos DB.
3. The API creates an audit event representing the relevant business activity.
4. The API publishes the event to the configured Azure Service Bus topic.
5. The trigger validates the message and schedules a Durable orchestration.
6. After scheduling succeeds, the Function invocation returns and the Service Bus binding completes the message automatically.
7. The Durable orchestration performs audit persistence. Failures at this stage must be retried and handled by Durable Functions; they cannot trigger redelivery of the completed Service Bus message.
8. Configure the subscription `MaxDeliveryCount` as **6** (one initial delivery plus five retries) for failures before the Function invocation returns. On exhaustion, Service Bus routes the message to its DLQ.
9. The DLQ function persists the original payload and metadata to Azure Table Storage; the binding completes the DLQ message automatically after the invocation succeeds.

Messages that cannot be deserialized fail the invocation and follow the subscription's normal retry/dead-letter policy. The Durable orchestration's later audit processing is outside the Service Bus message lock; orchestration failures do not cause the original Service Bus message to be redelivered.

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

## FR-04: Service Bus Consumption and Settlement

The .NET isolated Service Bus-triggered Function shall use automatic completion after a successful invocation. Exceptions must propagate so the Service Bus trigger can retry failed invocations.

The implementation shall define:

- Trigger execution behavior.
- The point at which the invocation returns and automatic message completion occurs.
- Message settlement and completion.
- Stable event identity and duplicate detection.
- Duplicate event detection.
- Failure handling and replay behavior.

Automatic completion covers successful invocation execution only. If work is handed to a Durable orchestration, later orchestration failures do not trigger Service Bus redelivery; the orchestration must provide its own retry and failure handling.

## FR-05: Audit Record Persistence

The consumer shall persist audit records into a dedicated Azure Cosmos DB audit container.

The audit container shall have its own throughput configuration, partition key, indexing policy, and capacity limits, independently of the primary application container where supported by the selected Cosmos DB configuration.

Audit writes shall use an idempotent persistence strategy where practical.

## FR-06: Bounded Processing Concurrency

The system shall limit concurrent processing at multiple levels:

- Number of messages prefetched and processed by each Function host.
- Number of concurrent trigger executions per instance.
- Maximum number of concurrent Service Bus-triggered invocations per host.
- Number of simultaneous database writes.

The active `host.json` baseline is four concurrent calls and zero prefetch per host, with dynamic concurrency disabled and a 15-minute maximum automatic Service Bus lock-renewal duration. The Function timeout remains five minutes and is a separate setting. The local rate limiter is per worker process, not global. Scale-out and worker-process count must be bounded against measured Cosmos capacity.

## FR-07: Retry and Failure Handling

The consumer shall support retry handling for transient errors, including temporary database unavailability, throttling, and intermittent network failures.

The retry policy shall specify:

- Subscription `MaxDeliveryCount` of 6, providing one initial attempt and up to five redeliveries.
- Treatment of HTTP 429 responses and `Retry-After` guidance.
- Classification of transient and permanent failures.
- Final disposition after retry exhaustion.

Cosmos DB SDK retries and application-level retries shall be coordinated to avoid excessive cumulative delays and retry amplification.

The Service Bus entity's configured delivery count is the authoritative retry limit; it is not a `host.json` setting. Messages that exhaust it shall be routed to the subscription DLQ. The trigger must not complete, abandon silently, or replace the original message with a success-shaped fallback when durable audit/history storage fails.

## FR-08: Dead-Letter Management

The system shall retain failed messages in the Service Bus DLQ for operational investigation and recovery.

A DLQ processing component shall record relevant metadata in Azure Table Storage, including the message identifier, event type, failure reason, dead-letter timestamp, retry information, and recovery status, subject to the chosen data model.

The solution shall support:

- Inspection of dead-letter messages.
- Classification of failure causes.
- Controlled replay or resubmission as a separately authorized operational workflow; archival alone is not replay.
- Prevention of duplicate audit writes during recovery.
- Cleanup of processed DLQ metadata.
- Monitoring of unprocessed and repeatedly failing messages.

Azure Table Storage shall contain the recoverable original payload and DLQ metadata before the original DLQ message is completed. Base64 encoding is not encryption; access, retention, and deletion must follow the payload classification policy. A Table/Cosmos archival failure must leave the DLQ message unsettled for retry.

## FR-09: Observability

The system shall provide structured logs, distributed tracing, metrics, and alerts covering the complete event lifecycle.

The implementation shall track at least:

- Events published and publication failures.
- Messages received and completed.
- Per-course event history states (`InProcess`, `Error`, `Completed`, and `DeadLettered`), message ID, delivery attempt, timestamp, notes, and error type.
- Audit write throughput and latency.
- Cosmos DB 429 responses and retry activity.
- Active Function instances and processing concurrency.
- Service Bus active-message count and oldest-message age.
- DLQ message count and age.
- End-to-end audit processing latency.
- Events potentially lost before successful publication or persistence.

## FR-10: Failed-Message Recovery and History API

The API shall expose paginated processing history for a course. History records shall be append-only and include event ID, message ID, status, attempt, timestamp, and safe operational notes. The solution shall define an operational workflow for replaying archived DLQ events.

Recovery shall be controlled and observable. It shall preserve event identity, maintain idempotency, and avoid unintentionally bypassing database throughput controls.

Where the normal consumer cannot recover a failure, the design shall evaluate a dedicated retry topic or queue with independent delivery and retry policies.

# 4. Non-Functional Requirements

## NFR-01: API Throughput

The primary CourseLibrary API shall support a 10-second peak burst of **10,000 requests**, equivalent to 1,000 requests/second during the burst, followed by a sustained rate of 500 requests/second.

The confirmed workload profile is that 100% of these requests produce an audit event.

The solution shall validate this profile through load testing.

## NFR-02: Audit Processing Throughput

The agreed workload profile is a 10-second peak of **1,000 events/second**, followed by **500 events/second sustained**. The previous target of 200 events/second is insufficient for this profile. Provisionally design and load-test for at least **600 completed events/second** to sustain the 500/second baseline and drain the burst backlog with headroom, subject to measured Cosmos RU/event and the p95 latency SLO.

The system shall distinguish between:

- Maximum allowed processing throughput.
- Sustainable throughput under normal database conditions.
- Burst ingestion rate.
- Recovery throughput after an outage.

The consumer shall not exceed the configured processing limit simply because additional Function instances become available.

The supplied workload profile is that 100% of API requests produce an event. At the API capacity target of 1,000 requests/second, peak event arrival is therefore 1,000/second. A 600-event/second consumer would leave 4,000 events queued after a 10-second burst from an empty queue, then drain that backlog at a net 100/second while the 500/second baseline continues, clearing it in about 40 seconds. A 200-event/second consumer would instead accumulate 300 events/second (1.08 million/hour) during the sustained load. These are deterministic queue estimates, not measured latency guarantees; validate the proposed rate and 2-minute p95 objective under load.

## NFR-03: Database Throughput Protection

The existing audit Cosmos DB estimate is approximately **1,000 RU/s**. The previous **4,286 RU/s** estimate assumed one 5-RU write per event and is not valid for the new implementation, which performs a duplicate-check read, an `InProcess` status create, and an audit-plus-`Completed` transactional batch. Measure the total RU charge per successfully persisted event (`R`) before approving capacity.

The architecture shall ensure that the consumer's aggregate database request rate remains within the sustainable capacity of the audit container.

The system shall handle throttling gracefully rather than relying on automatic horizontal scaling alone.

For planning only, reserve 30% of configured throughput for RU variability and competing operations. For measured `R` RU/event, required configured throughput is `events/second × R / 0.70`: the 500/s steady-state target requires `500R / 0.70` RU/s and 600/s backlog recovery requires `600R / 0.70` RU/s. At 1,000 RU/s, only 700 RU/s is available under that reserve; 500 events/s is supportable only if the full event path costs no more than 1.4 RU/event. No RU/event measurement exists, so neither the previous 140 events/s nor 4,286 RU/s estimate is a verified capacity. Measure the full path, including status records, transactional-batch overhead, retries, and indexing, before deployment.

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

The active runtime baseline is four concurrent Service Bus calls per host, zero prefetch, and a 20-operations/second local admission limit per worker process. Each successful event currently requires a Cosmos read, an `InProcess` status create, and an atomic audit-plus-`Completed` transactional batch; error paths add status writes. Therefore the former one-write RU and concurrency estimates are not valid for this implementation. The rate limiter is per worker and is not a fleet-wide cap. For example, if the measured end-to-end handler time were 200 ms, Little's Law gives 120 in-flight callbacks for 600 events/second; at four callbacks per host that is at least 30 equivalent single-process hosts, before accounting for scale delays, multiple worker processes, the additional writes, Cosmos throttling, or a database safety reserve. The actual required fleet size and Cosmos RU budget must be load-tested against the complete operation pattern. Configure and verify the Flex maximum scale-out externally; `host.json` alone is not a fleet-wide cap.

## NFR-05: Latency

The solution design shall define measurable latency targets for:

- API request processing.
- Event publication.
- Time from publication to consumption.
- Time from publication to successful audit persistence.
- Recovery after a transient failure.

The agreed end-to-end target is p95 event-publication-to-Cosmos-persistence latency of **2 minutes** under the specified 10-second burst and 500-event/second post-burst load. Validate it through performance testing; p95 latency cannot be guaranteed from RU/s arithmetic alone.

The relationship between throughput, latency, and concurrency shall be used for capacity planning:

**Concurrency ≈ Throughput × Average processing latency**

The calculation shall use consistent units and shall be supplemented with tail-latency measurements, including p95 and p99, where applicable.

For scale illustration, at 600 completed events/second, 100 ms, 200 ms, 250 ms, 500 ms, and 1 second of mean end-to-end callback time require approximately 60, 120, 150, 300, and 600 concurrent callbacks. With four callbacks per host, this is 15, 30, 38, 75, and 150 single-process hosts, respectively. These are Little's Law planning values only; the new multi-operation Cosmos write path, per-process rate limiter, scale response, partition hot spots, and retries must be measured before selecting the production cap.

## NFR-06: Eventual Consistency

Audit persistence shall be eventually consistent with the originating business operation.

The system shall define an acceptable delay between the successful business operation and the availability of its audit record.

The design shall acknowledge that an API operation can succeed while audit publication or persistence is delayed or fails.

The Service Bus consumer shall not silently lose accepted events. End-to-end zero loss additionally requires a transactional outbox because the current API publishes after the business database commit.

## NFR-07: Delivery Reliability and Idempotency

The architecture shall provide at-least-once Service Bus delivery and protect against duplicate messages.

The consumer shall use a stable event identifier and an idempotent persistence mechanism so that retries and replays do not create unintended duplicate audit records.

The consumer shall atomically create the audit record and `Completed` history entry in the same Cosmos container and `/courseId` partition. Service Bus settlement remains outside that database transaction; therefore the consumer must tolerate a commit followed by a lost settlement response by detecting the existing audit ID on redelivery. End-to-end zero loss additionally requires a transactional producer outbox; direct API publish after a business commit is not sufficient.

## NFR-08: Message Retention

The system shall target the following retention periods:

- Active Service Bus messages: up to 15 days.
- Dead-letter messages: up to 30 days.

The design shall validate the actual Service Bus tier and entity configuration against these requirements. Where a 30-day DLQ retention period cannot be configured directly, an alternative archival mechanism shall be specified.

The system shall define what happens when messages reach the end of their retention period without successful processing. DLQ archival must finish before DLQ settlement; no replay worker is currently provided by this design.

## NFR-09: Resource Efficiency and Cost

The architecture shall balance throughput, latency, availability, and operating cost.

The design shall evaluate the cost implications of:

- API and Function instance scaling.
- Service Bus tier and message operations.
- Cosmos DB provisioned or autoscale throughput.
- Append-only course event processing-history records.
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

The consumer shall support 500 events/second sustained and a 600 events/second processing rate for backlog recovery after a 10-second burst of 1,000 events/second. These are provisional capacity targets conditional on measured RU/event and the approved Cosmos budget.

The design shall determine the appropriate values for:

- Maximum concurrent message processing per Function instance.
- Maximum number of Function instances.
- Maximum concurrent Service Bus callbacks.
- Maximum event-admission rate per process and maximum Function scale-out.
- Message prefetch.
- Maximum outstanding work during database throttling.

Each successful event consumes two audit-write permits in the process-local limiter (one status create and one transactional batch) and one message permit for the duplicate-check read. With a 20-permit/second audit limiter, the limiter alone allows at most 10 events/second/process, so at least 60 single-process-equivalent workers are needed for 600 events/second. Four Service Bus callbacks per process provide 240 concurrent callback slots at that worker count; 600 events/second requires mean end-to-end handler time at or below 400 ms to fit those slots. These are necessary theoretical minima, not proof of actual capacity. Configure and verify the Flex maximum scale-out externally, and size it from measured RU/event and end-to-end callback latency. This scale limit is not configured in the repository.

For the agreed 10-second burst, an empty queue, 1,000 incoming events/second, and 600 completions/second, approximately 4,000 events remain queued at burst end. At 500 incoming/second and 600 completions/second afterward, net drain is 100/second and the queue clears in about 40 seconds, excluding handler overhead.

## 5.3 Cosmos DB Capacity

The existing 1,000 RU/s estimate supports 500 events/second only if measured RU/event is no more than 1.4 with a 30% reserve. The former 4,286 RU/s estimate is obsolete because it counted only one audit write. Measure RU/event for the complete handler before setting production throughput.

Capacity planning shall measure the RU cost of an individual audit write, including indexing overhead and any additional reads or transactional operations.

The design shall calculate sustainable write throughput as a function of actual RU consumption and the usable RU/s budget.

Use the following sensitivity table only as a mathematical illustration; `RU/event` must include the read, status create, audit create, completion status, indexing, and transactional-batch overhead:

| Measured RU per event | Rate at 1,000 RU/s with no reserve | Rate at provisional 700 RU/s (30% reserved) |
|---:|---:|---:|
| 5 | 200 events/s | 140 events/s |
| 10 | 100 events/s | 70 events/s |
| 20 | 50 events/s | 35 events/s |
| 50 | 20 events/s | 14 events/s |

Replace the illustrative RU/event with observed request charges from the complete successful event path. At 1,000 RU/s and 30% reserve, throughput is `700 / RU/event`; partition skew, retries, and competing workloads can reduce it further.

Autoscale or other supported throughput management shall be evaluated against the expected workload, while respecting partition-level limits and the platform's maximum configured throughput.

## 5.4 Database-Aware Backpressure

If the audit container is unavailable or repeatedly returns throttling responses, the consumer shall reduce or pause database-bound work instead of continuing to scale out without limit.

Service Bus shall serve as the durable buffer within its configured capacity and retention constraints.

The design shall specify how processing resumes when the database recovers and how backlog-drain rates are controlled.

# 6. Message Retry and Delivery Requirements

The solution shall define the following retry behavior:

1. Service Bus message delivery, configured for one initial attempt and five redeliveries.
2. Azure Cosmos DB SDK retries for throttling and transient failures.
3. Operational replay of archived DLQ messages, when implemented and authorized.

The retry design shall prevent excessive cumulative delays and duplicate side effects.

Failures before orchestration scheduling must propagate from the Function invocation for Service Bus retry. The trigger's automatic completion occurs after successful scheduling, not after Cosmos persistence. The Service Bus lock-renewal window is 15 minutes; the Function execution timeout remains five minutes. Failures after scheduling require Durable retry handling and do not return the original message to Service Bus.

# 7. Assumptions

The following assumptions apply unless revised during architecture review.

**ASM-01:** The primary application is implemented using ASP.NET Core Web API on .NET.

**ASM-02:** Azure Service Bus topics and subscriptions are the primary transport mechanism for audit events.

**ASM-03:** The Service Bus-triggered Function directly persists audit and history records before completing a message; Durable Functions are not used for new messages.

**ASM-04:** Azure Cosmos DB is the audit record store, using a container separate from the primary application data container.

**ASM-05:** Azure Table Storage is used to retain DLQ processing metadata for investigation and recovery.

**ASM-06:** Audit processing is asynchronous. The API is not required to wait for the audit record to be persisted before responding, except where a future business requirement specifies otherwise.

**ASM-07:** 100% of API requests produce audit events. The load profile is a 10-second burst of 1,000 events/second followed by 500 events/second sustained; the proposed consumer capacity is at least 600 completed events/second.

**ASM-08:** The existing audit container estimate is 1,000 RU/s. Its sufficiency cannot be determined until RU/event is measured for the full multi-operation processing path; the former 4,286 RU/s single-write estimate is obsolete.

**ASM-09:** Audit processing delay is acceptable, but accepted Service Bus events must not be silently lost. The API's direct-publish dual-write gap remains until a transactional outbox is implemented.

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

Confirm the Azure hosting plans, Functions runtime model, Service Bus tier, subscription delivery count, and applicable scaling limits.

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

**CON-02:** Audit processing must be bounded by measured Cosmos capacity and must support the agreed 500 events/second sustained arrival, a 1,000 events/second 10-second burst, and the proposed 600 events/second recovery rate if validated.

**CON-03:** Measure RU/event for the complete read/status/transactional-write path and set Cosmos throughput to `required events/second × RU/event / 0.70` for a 30% reserve. Do not deploy based on the previous 4,286 RU/s one-write assumption.

**CON-04:** Function horizontal scaling must not cause uncontrolled Cosmos DB request growth or persistent 429 throttling.

**CON-05:** The system must operate asynchronously and tolerate delayed audit persistence.

**CON-06:** The Service Bus message-retention and DLQ-retention requirements must be reconciled with the selected service tier and supported configuration.

**CON-07:** Retrying, rescheduling, or replaying an event must not cause unintended duplicate audit records.

**CON-08:** A Service Bus message must not be completed until its audit record and `Completed` history entry are durable in Cosmos DB.

**CON-09:** The system must account for transient and permanent failures across the API, Service Bus, and Cosmos DB.

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
5. Direct Service Bus processing, settlement, and append-only course history design.
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
- A transient processing failure leaves the message unsettled; after one initial attempt and five redeliveries it is retained in the subscription DLQ.
- A failure to archive a DLQ message leaves that original DLQ message unsettled.
- Audit and completed-history writes are atomic and duplicate-safe.
- `GET /api/v1/courses/{courseId}/history` returns paginated, course-filtered status entries.
- DLQ messages can be inspected, tracked, and replayed safely.
- The configured message retention meets the approved retention requirements or an approved alternative is implemented.
- Monitoring detects publication failures, excessive audit latency, growing backlog, database throttling, and DLQ accumulation.
- Producer-side delivery is protected by a transactional outbox before the system may claim end-to-end zero loss; the current direct-publish dual-write gap is a release blocker for that claim.
- Performance, resilience, and failure-recovery tests demonstrate that the architecture meets the agreed service-level objectives.
