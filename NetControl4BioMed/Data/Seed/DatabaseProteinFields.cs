using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;

namespace NetControl4BioMed.Data.Seed
{
    /// <summary>
    /// Represents the seed data for the database protein fields.
    /// </summary>
    public class DatabaseProteinFields
    {
        /// <summary>
        /// Represents the database protein fields to be seeded.
        /// </summary>
        public static ICollection<DatabaseProteinField> Seed { get; } = new List<DatabaseProteinField>
        {
            // The protein fields in the DrugBank database.
            new DatabaseProteinField
            {
                Id = "e34b7069-9615-4542-94ac-307f39e16565",
                DateTimeCreated = DateTime.UtcNow,
                Name = "DrugBank Drug ID",
                Description = "The unique identifier of a drug in the DrugBank database.",
                IsSearchable = false,
                Url = "https://www.drugbank.ca/drugs/",
                DatabaseId = "d7182750-be72-40be-9ac2-ba62f2fe4172"
            },
            new DatabaseProteinField
            {
                Id = "e79c519a-e42a-4733-aafa-f105dde8aab7",
                DateTimeCreated = DateTime.UtcNow,
                Name = "DrugBank Drug Name",
                Description = "The name of a drug in the DrugBank database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "d7182750-be72-40be-9ac2-ba62f2fe4172"
            },
            // The protein fields in the Ensembl database.
            new DatabaseProteinField
            {
                Id = "ae539cf2-305d-442e-94ed-0fed9a391f2f",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Ensembl ID",
                Description = "The unique identifier of a gene in the Ensembl database.",
                IsSearchable = true,
                Url = "https://www.ensembl.org/Homo_sapiens/Gene/Summary?g=",
                DatabaseId = "17bebf9e-6a58-464f-8180-0eb913a27932"
            },
            // The protein fields in the HGNC database.
            new DatabaseProteinField
            {
                Id = "197aee65-46ae-4782-a58d-7ce04c4d7f12",
                DateTimeCreated = DateTime.UtcNow,
                Name = "HGNC ID",
                Description = "The unique identifier of a gene in the HGNC database.",
                IsSearchable = true,
                Url = "https://www.genenames.org/data/gene-symbol-report/#!/hgnc_id/",
                DatabaseId = "11b63176-a16f-434c-8a36-e986460afa13"
            },
            new DatabaseProteinField
            {
                Id = "27687402-74fa-4b37-a05f-ca4687302228",
                DateTimeCreated = DateTime.UtcNow,
                Name = "HGNC Symbol",
                Description = "The symbol of a gene in the HGNC database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "11b63176-a16f-434c-8a36-e986460afa13"
            },
            new DatabaseProteinField
            {
                Id = "46e37350-d022-448d-afe8-52ffe3778a09",
                DateTimeCreated = DateTime.UtcNow,
                Name = "HGNC Name",
                Description = "The name of a gene in the HGNC database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "11b63176-a16f-434c-8a36-e986460afa13"
            },
            new DatabaseProteinField
            {
                Id = "49a35149-21a8-4286-a28d-372faeadae43",
                DateTimeCreated = DateTime.UtcNow,
                Name = "HGNC Alias",
                Description = "The alias of a gene in the HGNC database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "11b63176-a16f-434c-8a36-e986460afa13"
            },
            // The protein fields in the InnateDB database.
            new DatabaseProteinField
            {
                Id = "d575fa61-b5fb-405c-82c5-48ca9500f954",
                DateTimeCreated = DateTime.UtcNow,
                Name = "InnateDB ID",
                Description = "The unique identifier of a gene in the InnateDB database.",
                IsSearchable = true,
                Url = "https://www.innatedb.com/getGeneCard.do?id=",
                DatabaseId = "ae5e13e7-62c7-4c57-ad04-cc83db996d5d"
            },
            // The protein fields in the KEGG database.
            new DatabaseProteinField
            {
                Id = "e10c5e37-025f-412c-9de1-7e6f8a6552cc",
                DateTimeCreated = DateTime.UtcNow,
                Name = "KEGG ID",
                Description = "The unique identifier of a gene in the KEGG database.",
                IsSearchable = true,
                Url = "https://www.genome.jp/dbget-bin/www_bget?hsa:",
                DatabaseId = "c7da2735-bd4b-4f1f-a855-a9da130583ee"
            },
            // The protein fields in the NCBI database.
            new DatabaseProteinField
            {
                Id = "cb214a7e-4de7-4014-987b-9cb64d0e2012",
                DateTimeCreated = DateTime.UtcNow,
                Name = "NCBI ID",
                Description = "The unique identifier of a gene in the NCBI database.",
                IsSearchable = true,
                Url = "https://www.ncbi.nlm.nih.gov/gene/",
                DatabaseId = "4f8e2ba7-691d-476e-b121-7af20b158f96"
            },
            // The protein fields in the UniProt database.
            new DatabaseProteinField
            {
                Id = "c6947ee0-f6cd-4643-85c3-2f87df12e22a",
                DateTimeCreated = DateTime.UtcNow,
                Name = "UniProt ID",
                Description = "The unique identifier of a gene in the UniProt database.",
                IsSearchable = true,
                Url = "https://www.uniprot.org/uniprot/",
                DatabaseId = "39757dcc-80ee-4034-992b-f221df8a71e6"
            },
            new DatabaseProteinField
            {
                Id = "c9123bef-0bd5-4362-9cd7-9a42cad0d959",
                DateTimeCreated = DateTime.UtcNow,
                Name = "UniProt Title",
                Description = "The title of a gene in the UniProt database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "39757dcc-80ee-4034-992b-f221df8a71e6"
            }
        };
    }
}
