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
                Description = "The display name of the node.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "08dff7db-d065-454b-9e4d-afef087c2b05"
            },
            // The node fields in the Ensembl database.
            new DatabaseNodeField
            {
                Id = "8cd819ab-6e24-4d73-b808-090d62b06216",
                DateTimeCreated = DateTime.Now,
                Name = "Ensembl ID",
                Description = "The protein unique identifier(s) in the Ensembl database.",
                IsSearchable = true,
                Url = "https://www.ensembl.org/Homo_sapiens/Gene/Summary?g=",
                DatabaseId = "8482602c-c0b4-4866-ba9e-7ad3266d03b9"
            },
            // The node fields in the HGNC database.
            new DatabaseNodeField
            {
                Id = "f9dc5b1c-2cd2-4f07-ac4e-1200d3f02338",
                DateTimeCreated = DateTime.Now,
                Name = "HGNC Name",
                Description = "The protein name, as set by HGNC.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "f16801a9-cc2a-4825-ac85-5ee60555439a"
            },
            new DatabaseNodeField
            {
                Id = "981ae97c-326c-4955-8691-48e15508f499",
                DateTimeCreated = DateTime.Now,
                Name = "HGNC ID",
                Description = "The unique identifier(s) in the HGNC database.",
                IsSearchable = true,
                Url = "https://www.genenames.org/data/gene-symbol-report/#!/hgnc_id/",
                DatabaseId = "f16801a9-cc2a-4825-ac85-5ee60555439a"
            },
            // The node fields in the InnateDB database.
            new DatabaseNodeField
            {
                Id = "8ee6c0ca-936a-448d-a49f-4cb46893b23f",
                DateTimeCreated = DateTime.Now,
                Name = "InnateDB ID",
                Description = "The protein unique identifier(s) in the InnateDB database.",
                IsSearchable = true,
                Url = "https://www.innatedb.com/getGeneCard.do?id=",
                DatabaseId = "e833d529-bceb-4cac-808e-22acf5a5bf0a"
            },
            // The node fields in the KEGG database.
            new DatabaseNodeField
            {
                Id = "eb9a4caa-cbe9-4a03-a9bf-1740103d9f49",
                DateTimeCreated = DateTime.Now,
                Name = "KEGG ID",
                Description = "The protein unique identifier(s) in the KEGG database.",
                IsSearchable = true,
                Url = "https://www.genome.jp/dbget-bin/www_bget?",
                DatabaseId = "8364ed3b-cf03-40d8-a28d-1405c365cdee"
            },
            new DatabaseNodeField
            {
                Id = "cb7f1820-1e37-473b-ba82-b0e2776d46cc",
                DateTimeCreated = DateTime.Now,
                Name = "KEGG Type",
                Description = "The protein type (gene, compound or group) in the KEGG database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "8364ed3b-cf03-40d8-a28d-1405c365cdee"
            },
            // The node fields in the PubChem database.
            new DatabaseNodeField
            {
                Id = "eeea2046-1798-42dc-9a30-785e7a3cd922",
                DateTimeCreated = DateTime.Now,
                Name = "PubChem ID",
                Description = "The protein unique identifier(s) in the PubChem database.",
                IsSearchable = true,
                Url = "https://pubchem.ncbi.nlm.nih.gov/gene/",
                DatabaseId = "5386671e-818d-4eb3-a1d5-b0c93e39f786"
            },
            // The node fields in the UniProt database.
            new DatabaseNodeField
            {
                Id = "0347512f-dcd7-4491-bc7f-29123070eab8",
                DateTimeCreated = DateTime.Now,
                Name = "UniProt ID",
                Description = "The protein unique identifier(s) in the UniProt database.",
                IsSearchable = true,
                Url = "https://www.uniprot.org/uniprot/",
                DatabaseId = "458258dd-794f-4eec-9fe4-bc865b04f585"
            }
        };
    }
}
