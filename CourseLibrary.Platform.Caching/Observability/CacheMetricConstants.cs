using System;
using System.Collections.Generic;
using System.Text;

namespace CourseLibrary.Platform.Caching.Observability;

public static class CacheMetricConstants
{
    public const string MeterName = "CourseLibrary.Platform.Caching";
    public const string LookupsInstrumentName = "course_library.cache.lookups";
    public const string HitsInstrumentName = "course_library.cache.hits";
    public const string MissesInstrumentName = "course_library.cache.misses";

    
}