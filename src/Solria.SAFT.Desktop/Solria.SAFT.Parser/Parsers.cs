using SolRIA.SAFT.Parser.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace SolRIA.SAFT.Parser;

public static class Parsers
{
    public static List<ValidationError> Validations { get; private set; } = new List<ValidationError>();

    public static bool StringEquals(string str1, string str2)
    {
        if (str1 == null || str2 == null)
            return false;

        return str1.Equals(str2, StringComparison.OrdinalIgnoreCase);
    }

    /// 
    /// Parsers
    /// 

    public static DateTime ParseDate(string value, string pk, string id, string field, Type baseType, string supPk = null)
    {
        if (DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            return date;

        Validations.Add(new ValidationError { UID = pk, Description = "Não foi possível ler a data", Field = field, Value = value, TypeofError = baseType, SupUID = supPk });

        return DateTime.MinValue;
    }
    public static DateTime ParseDateTime(string value, string pk, string id, string field, Type baseType, string supPk = null)
    {
        if (DateTime.TryParseExact(value, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            return date;

        Validations.Add(new ValidationError { UID = pk, Description = "Não foi possível ler a data", Field = field, Value = value, TypeofError = baseType, SupUID = supPk });

        return DateTime.MinValue;
    }
    public static decimal ParseDecimal(string value, string pk, string id, string field, Type baseType, string supPk = null)
    {
        if (decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal credit))
            return credit;

        Validations.Add(new ValidationError { UID = pk, Description = "Não foi possível ler o valor", Field = field, Value = value, TypeofError = baseType, SupUID = supPk });

        return default;
    }
    public static T ParseEnum<T>(string value, string pk, string id, string field, Type baseType, string supPk = null) where T : struct
    {
        if (Enum.TryParse(value, out T type))
            return type;

        Validations.Add(new ValidationError { UID = pk, Description = "Não foi possível ler o valor", Field = field, Value = value, TypeofError = baseType, SupUID = supPk });

        return default;
    }
}
