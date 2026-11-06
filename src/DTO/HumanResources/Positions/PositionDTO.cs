namespace HRM.DTO
{
    public class PositionDTO
    {
        public string positionId { get; set; }
        public string positionName { get; set; }
        public string description { get; set; }
        public decimal positionAllowance { get; set; }

        public PositionDTO() { }

        public PositionDTO(string positionId, string positionName, string description, decimal positionAllowance)
        {
            this.positionId = positionId;
            this.positionName = positionName;
            this.description = description;
            this.positionAllowance = positionAllowance;
        }
    }
}
