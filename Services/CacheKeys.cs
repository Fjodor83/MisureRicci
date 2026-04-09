namespace MisureRicci.Services
{
    public static class CacheKeys
    {
        public const string MeasurementTypesActive = "measurement_types_active_v1";
        public const string MeasurementTypesAll = "measurement_types_all_v1";
        public const string FieldDefinitionsPrefix = "measurement_fields_type_";
        public const string NegozioAll = "negozi_all_v1";
        public const string DashboardKpiPrefix = "dashboard_kpi_v1_";

        public static string FieldDefinitions(int typeId, bool onlyActive) =>
            $"{FieldDefinitionsPrefix}{typeId}_{(onlyActive ? "active" : "all")}";
    }
}
