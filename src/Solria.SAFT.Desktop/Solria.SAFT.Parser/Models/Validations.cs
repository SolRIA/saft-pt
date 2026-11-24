using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SolRIA.SAFT.Parser.Models;

public static partial class Validations
{
    private static readonly HashSet<string> ValidTwoDigitNifPrefixes = new(StringComparer.Ordinal)
    {
        "45", "70", "71", "72", "74", "75", "77", "78", "79", "90", "91", "98", "99"
    };

    private static readonly HashSet<string> ValidTaxExemptionCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "M01", "M02", "M03", "M04", "M05", "M06", "M07", "M09", "M10", "M11",
        "M12", "M13", "M14", "M15", "M16", "M19", "M20", "M21", "M25", "M26",
        "M30", "M31", "M32", "M33", "M34", "M40", "M41", "M42", "M43", "M99"
    };

    public static bool CheckTaxRegistrationNumber(string taxRegistrationNumber)
    {
        if (string.IsNullOrEmpty(taxRegistrationNumber))
            return false;

        if (taxRegistrationNumber.Length != 9 || !IsNumeric(taxRegistrationNumber))
            return false;

        char firstChar = taxRegistrationNumber[0];
        bool validFirstDigit = firstChar is '1' or '2' or '3' or '5' or '6' or '8';
        bool validTwoDigitPrefix = ValidTwoDigitNifPrefixes.Contains(taxRegistrationNumber[..2]);
        if (!validFirstDigit && !validTwoDigitPrefix)
            return false;

        int checkDigit = 0;
        for (int i = 0; i < 8; i++)
        {
            checkDigit += (taxRegistrationNumber[i] - '0') * (9 - i);
        }

        checkDigit = 11 - (checkDigit % 11);
        if (checkDigit >= 10)
            checkDigit = 0;

        return checkDigit == (taxRegistrationNumber[8] - '0');
    }

    public static bool IsValidTaxExemptionCode(string code)
    {
        return !string.IsNullOrWhiteSpace(code) && ValidTaxExemptionCodes.Contains(code.Trim());
    }

    public static bool IsRevokedTaxExemptionCode(string code)
    {
        return string.Equals(code?.Trim(), "M08", StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryParseDocumentNumber(string docNo, out string docType, out string series, out int number)
    {
        docType = null;
        series = null;
        number = 0;

        if (string.IsNullOrWhiteSpace(docNo))
            return false;

        int spaceIndex = docNo.IndexOf(' ');
        if (spaceIndex <= 0 || spaceIndex >= docNo.Length - 1)
            return false;

        int slashIndex = docNo.IndexOf('/', spaceIndex + 1);
        if (slashIndex <= spaceIndex || slashIndex >= docNo.Length - 1)
            return false;

        docType = docNo[..spaceIndex];
        series = docNo.Substring(spaceIndex + 1, slashIndex - spaceIndex - 1);
        return int.TryParse(docNo[(slashIndex + 1)..], out number) && number >= 0;
    }

    public static ValidationError ValidateAtcud(string atcud, string documentNo, string field, Type typeofError, string uid)
    {
        if (string.IsNullOrWhiteSpace(atcud))
        {
            return new ValidationError
            {
                Description = "ATCUD não pode ser vazio.",
                Field = field,
                FileID = documentNo,
                Value = atcud,
                TypeofError = typeofError,
                UID = uid
            };
        }

        if (atcud.Length > 100)
        {
            return new ValidationError
            {
                Description = "ATCUD com tamanho superior ao permitido (100 caracteres).",
                Field = field,
                FileID = documentNo,
                Value = atcud,
                TypeofError = typeofError,
                UID = uid
            };
        }

        if (atcud == "0")
            return null;

        string[] parts = atcud.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return new ValidationError
            {
                Description = "ATCUD deveria ser formado pela concatenação dos campos «CodigodeValidação-NumeroSequencial».",
                Field = field,
                FileID = documentNo,
                Value = atcud,
                TypeofError = typeofError,
                UID = uid
            };
        }

        if (parts[0].Length < 8 || !AtcudValidationCodeRegex().IsMatch(parts[0]))
        {
            return new ValidationError
            {
                Description = "CodigodeValidação do ATCUD inválido ou com tamanho incorrecto.",
                Field = field,
                FileID = documentNo,
                Value = atcud,
                TypeofError = typeofError,
                UID = uid
            };
        }

        if (!int.TryParse(parts[1], out int atcudSeq) || atcudSeq <= 0)
        {
            return new ValidationError
            {
                Description = "Número sequencial do ATCUD inválido.",
                Field = field,
                FileID = documentNo,
                Value = atcud,
                TypeofError = typeofError,
                UID = uid
            };
        }

        if (TryParseDocumentNumber(documentNo, out _, out _, out int docSeq) && atcudSeq != docSeq)
        {
            return new ValidationError
            {
                Description = $"Número sequencial do ATCUD ({atcudSeq}) não coincide com o número do documento ({docSeq}).",
                Field = field,
                FileID = documentNo,
                Value = atcud,
                TypeofError = typeofError,
                UID = uid
            };
        }

        return null;
    }

    public static bool IsNumeric(string inputString)
    {
        return !string.IsNullOrEmpty(inputString) && IsNumericRegex().IsMatch(inputString);
    }

    [GeneratedRegex("^[0-9]+$")]
    private static partial Regex IsNumericRegex();

    [GeneratedRegex("^[A-Za-z0-9]+$")]
    private static partial Regex AtcudValidationCodeRegex();
}
