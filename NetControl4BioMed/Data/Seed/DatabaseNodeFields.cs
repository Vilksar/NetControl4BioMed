using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Seed
{
    /// <summary>
    /// Represents the seed data for the database node fields.
    /// </summary>
    public class DatabaseNodeFields
    {
        /// <summary>
        /// Represents the database node fields to be seeded.
        /// </summary>
        public static ICollection<DatabaseNodeField> Seed { get; } = new List<DatabaseNodeField>
        {
            // The node fields in the generic database.
            new DatabaseNodeField
            {
                Id = "f5981a1b-5644-4e5b-ba4d-60db4dcc9d4d",
                DateTimeCreated = DateTime.Now,
                Name = "Generic Name",
                Description = "The name of a generic node.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "08dff7db-d065-454b-9e4d-afef087c2b05"
            },
            // The node fields in the DrugBank database.
            new DatabaseNodeField
            {
                Id = "e34b7069-9615-4542-94ac-307f39e16565",
                DateTimeCreated = DateTime.Now,
                Name = "DrugBank Drug ID",
                Description = "The unique identifier of a drug in the DrugBank database.",
                IsSearchable = false,
                Url = "https://www.drugbank.ca/drugs/",
                DatabaseId = "d7182750-be72-40be-9ac2-ba62f2fe4172"
            },
            new DatabaseNodeField
            {
                Id = "e79c519a-e42a-4733-aafa-f105dde8aab7",
                DateTimeCreated = DateTime.Now,
                Name = "DrugBank Drug Name",
                Description = "The name of a drug in the DrugBank database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "d7182750-be72-40be-9ac2-ba62f2fe4172"
            },
            // The node fields in the Ensembl database.
            new DatabaseNodeField
            {
                Id = "ae539cf2-305d-442e-94ed-0fed9a391f2f",
                DateTimeCreated = DateTime.Now,
                Name = "Ensembl ID",
                Description = "The unique identifier of a gene in the Ensembl database.",
                IsSearchable = true,
                Url = "https://www.ensembl.org/Homo_sapiens/Gene/Summary?g=",
                DatabaseId = "17bebf9e-6a58-464f-8180-0eb913a27932"
            },
            // The node fields in the HGNC database.
            new DatabaseNodeField
            {
                Id = "197aee65-46ae-4782-a58d-7ce04c4d7f12",
                DateTimeCreated = DateTime.Now,
                Name = "HGNC ID",
                Description = "The unique identifier of a gene in the HGNC database.",
                IsSearchable = true,
                Url = "https://www.genenames.org/data/gene-symbol-report/#!/hgnc_id/",
                DatabaseId = "11b63176-a16f-434c-8a36-e986460afa13"
            },
            new DatabaseNodeField
            {
                Id = "27687402-74fa-4b37-a05f-ca4687302228",
                DateTimeCreated = DateTime.Now,
                Name = "HGNC Symbol",
                Description = "The symbol of a gene in the HGNC database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "11b63176-a16f-434c-8a36-e986460afa13"
            },
            new DatabaseNodeField
            {
                Id = "46e37350-d022-448d-afe8-52ffe3778a09",
                DateTimeCreated = DateTime.Now,
                Name = "HGNC Name",
                Description = "The name of a gene in the HGNC database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "11b63176-a16f-434c-8a36-e986460afa13"
            },
            new DatabaseNodeField
            {
                Id = "49a35149-21a8-4286-a28d-372faeadae43",
                DateTimeCreated = DateTime.Now,
                Name = "HGNC Alias",
                Description = "The alias of a gene in the HGNC database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "11b63176-a16f-434c-8a36-e986460afa13"
            },
            // The node fields in the InnateDB database.
            new DatabaseNodeField
            {
                Id = "d575fa61-b5fb-405c-82c5-48ca9500f954",
                DateTimeCreated = DateTime.Now,
                Name = "InnateDB ID",
                Description = "The unique identifier of a gene in the InnateDB database.",
                IsSearchable = true,
                Url = "https://www.innatedb.com/getGeneCard.do?id=",
                DatabaseId = "ae5e13e7-62c7-4c57-ad04-cc83db996d5d"
            },
            // The node fields in the KEGG database.
            new DatabaseNodeField
            {
                Id = "e10c5e37-025f-412c-9de1-7e6f8a6552cc",
                DateTimeCreated = DateTime.Now,
                Name = "KEGG ID",
                Description = "The unique identifier of a gene in the KEGG database.",
                IsSearchable = true,
                Url = "https://www.genome.jp/dbget-bin/www_bget?",
                DatabaseId = "c7da2735-bd4b-4f1f-a855-a9da130583ee"
            },
            // The node fields in the NCBI database.
            new DatabaseNodeField
            {
                Id = "cb214a7e-4de7-4014-987b-9cb64d0e2012",
                DateTimeCreated = DateTime.Now,
                Name = "NCBI ID",
                Description = "The unique identifier of a gene in the NCBI database.",
                IsSearchable = true,
                Url = "https://www.ncbi.nlm.nih.gov/gene/",
                DatabaseId = "4f8e2ba7-691d-476e-b121-7af20b158f96"
            },
            // The node fields in the UniProt database.
            new DatabaseNodeField
            {
                Id = "c6947ee0-f6cd-4643-85c3-2f87df12e22a",
                DateTimeCreated = DateTime.Now,
                Name = "UniProt ID",
                Description = "The unique identifier of a gene in the UniProt database.",
                IsSearchable = true,
                Url = "https://www.uniprot.org/uniprot/",
                DatabaseId = "39757dcc-80ee-4034-992b-f221df8a71e6"
            },
            new DatabaseNodeField
            {
                Id = "c9123bef-0bd5-4362-9cd7-9a42cad0d959",
                DateTimeCreated = DateTime.Now,
                Name = "UniProt Title",
                Description = "The title of a gene in the UniProt database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "39757dcc-80ee-4034-992b-f221df8a71e6"
            }
        };
    }
}
