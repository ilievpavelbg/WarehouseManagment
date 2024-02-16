using System.ComponentModel;

namespace WarehouseManagment.Data
{
    public enum Size
    {
        XS,
        [Description("XS/S")]
        XSS,
        S,
        [Description("S/M")]
        SM,
        M,
        [Description("M/L")]
        ML,
        L,
        XL,
        [Description("XL/XXL")]
        XLXXL,
        XXL,
        [Description("XXXL/4XL")]
        XXXL4XL,
        [Description("XS/S/M")]
        XSSM,
        [Description("L/XL/XXL")]
        LXLXXL,
        UNI,
        Oversize,
        [Description("24")]
        a = 24,
        [Description("25")]
        b = 25,
        [Description("26")]
        c = 26,
        [Description("27")]
        d = 27,
        [Description("28")]
        e = 28,
        [Description("29")]
        f = 29,
        [Description("30")]
        g = 30,
        [Description("31")]
        h = 31,
        [Description("32")]
        i = 32,
        [Description("33")]
        j = 33,
        [Description("34")]
        k = 34,
        [Description("35")]
        l = 35,
        [Description("36")]
        m = 36,
        [Description("37")]
        n = 37,
        [Description("38")]
        o = 38,
        [Description("40")]
        p = 40,
        [Description("42")]
        r = 42
    }
}
