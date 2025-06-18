namespace top.ebiz.service.ModelsTOPEBizCreateTripEntities
{
    public partial class BZ_BUDGET_APPROVER_CONDITION
    {
        public string? SEQ { get; set; }
        public string? APPROVER_TYPE { get; set; }
        public string? SPECIAL_CONDITION_ROLE { get; set; }
        public string? SPECIAL_CONDITION_FUNCTION { get; set; }
        public string? BUDGET_SYMBOL { get; set; }
        public decimal? BUDGET_LIMIT { get; set; }

        public string? LINE_LEVEL1 { get; set; }
        public string? LINE_LEVEL2 { get; set; }
        public string? CAP_LEVEL1 { get; set; }
        public string? CAP_LEVEL2 { get; set; }
        public string? CAP_LEVEL3 { get; set; }
        public string? REMARK { get; set; }
    }
}
