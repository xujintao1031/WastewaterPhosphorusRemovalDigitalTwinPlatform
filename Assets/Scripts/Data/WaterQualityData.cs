namespace Data
{
    /// <summary>
    /// Common water quality fields shared across all sheets.
    /// </summary>
    public class WaterQualityBase
    {
        [Column("生化工段")]
        public string Stage;

        [Column("COD (mg/L)")]
        public float COD;

        [Column("SS (mg/L)")]
        public float SS;

        [Column("氨氮 (mg/L)")]
        public float AmmoniaNitrogen;

        [Column("硝态氮 (mg/L)")]
        public float NitrateNitrogen;

        [Column("总氮 (mg/L)")]
        public float TotalNitrogen;

        [Column("总磷 (mg/L)")]
        public float TotalPhosphorus;
    }

    /// <summary>每日水质数据 — 时间字段: 日期</summary>
    [Sheet("每日水质数据")]
    public class DailyWaterQuality : WaterQualityBase
    {
        [Column("日期")]
        public string Date;
    }

    /// <summary>每周水质数据 — 时间字段: 年份 + 周数</summary>
    [Sheet("每周水质数据")]
    public class WeeklyWaterQuality : WaterQualityBase
    {
        [Column("年份")]
        public string Year;

        [Column("周数")]
        public string Week;
    }

    /// <summary>每月水质数据 — 时间字段: 年份 + 月份</summary>
    [Sheet("每月水质数据")]
    public class MonthlyWaterQuality : WaterQualityBase
    {
        [Column("年份")]
        public string Year;

        [Column("月份")]
        public string Month;
    }
}
