using System.Text.RegularExpressions;

namespace SolRIA.SAFT.Parser.Models;

public static partial class Validations
{
    public static bool CheckTaxRegistrationNumber(string taxRegistrationNumber)
    {
        if (string.IsNullOrEmpty(taxRegistrationNumber))
            return false;

        if (IsNumeric(taxRegistrationNumber) == false || taxRegistrationNumber.Length != 9) return false;

        int firstDigit = taxRegistrationNumber[0];
        if (char.IsDigit(taxRegistrationNumber[0]) == false)
            return false;

        var checkDigit = firstDigit * 9;
        for (var i = 2; i <= 8; i++)
        {
            if (char.IsDigit(taxRegistrationNumber[i]) == false)
                return false;

            checkDigit += taxRegistrationNumber[i - 1] * (10 - i);
        }
        checkDigit = 11 - checkDigit % 11;

        if (checkDigit >= 10)
            checkDigit = 0;

        if (checkDigit == char.GetNumericValue(taxRegistrationNumber[8]))
            return true;

        return false;
    }

    public static bool IsNumeric(string inputString)
    {
        return IsNumericRegex().IsMatch(inputString);
    }

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex IsNumericRegex();
}
