using NetControl4BioMed.Data.Enumerations;
using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Seed
{
    /// <summary>
    /// Represents the seed data for the database interaction fields.
    /// </summary>
    public class DatabaseInteractionFields
    {
        /// <summary>
        /// Represents the database interaction fields to be seeded.
        /// </summary>
        public static ICollection<DatabaseInteractionField> Seed { get; } = new List<DatabaseInteractionField>
        {
            // The interaction fields in the InnateDb database.
            new DatabaseInteractionField
            {
                Id = "3025373a-3e4f-4ff3-ae9c-8004fefe1e5a",
                DateTimeCreated = DateTime.UtcNow,
                Name = "InnateDB Interaction ID",
                Description = "The unique identifier of an interaction in the InnateDB database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "ae5e13e7-62c7-4c57-ad04-cc83db996d5d"
            },
            new DatabaseInteractionField
            {
                Id = "1ef7f744-1950-4cd8-8f3d-be5e9fc865aa",
                DateTimeCreated = DateTime.UtcNow,
                Name = "InnateDB Detection Method",
                Description = "The detection method of an interaction in the InnateDB database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "ae5e13e7-62c7-4c57-ad04-cc83db996d5d"
            },
            new DatabaseInteractionField
            {
                Id = "2bbaf190-4163-44ee-b5ad-8857d88fb807",
                DateTimeCreated = DateTime.UtcNow,
                Name = "InnateDB Type",
                Description = "The type of an interaction in the InnateDB database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "ae5e13e7-62c7-4c57-ad04-cc83db996d5d"
            },
            new DatabaseInteractionField
            {
                Id = "2071289a-9f19-4ccc-8e45-e2910ea148ed",
                DateTimeCreated = DateTime.UtcNow,
                Name = "InnateDB PubMed ID",
                Description = "The PubMed identifier of an interaction in the InnateDB database.",
                IsSearchable = false,
                Url = "http://www.ncbi.nlm.nih.gov/pubmed/",
                DatabaseId = "ae5e13e7-62c7-4c57-ad04-cc83db996d5d"
            },
            // The interaction fields in the KEGG database.
            new DatabaseInteractionField
            {
                Id = "4f336cea-4a9a-4ec4-92af-30220ae1e4af",
                DateTimeCreated = DateTime.UtcNow,
                Name = "KEGG Type",
                Description = "The type of an interaction in the KEGG database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "c7da2735-bd4b-4f1f-a855-a9da130583ee"
            },
            // The interaction fields in the Omnipath database.
            new DatabaseInteractionField
            {
                Id = "00d28f76-a6ee-4c74-81f6-4fcea29b5784",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Omnipath Type",
                Description = "The type of an interaction in the OmniPath database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "19140070-fb5f-46c8-90bc-34084bccdf39"
            },
            // The interaction fields in the SIGNOR database.
            new DatabaseInteractionField
            {
                Id = "dc9f99c8-b224-4601-a544-e228a345efd8",
                DateTimeCreated = DateTime.UtcNow,
                Name = "SIGNOR Interaction ID",
                Description = "The unique identifier of an interaction in the SIGNOR database.",
                IsSearchable = true,
                Url = null,
                DatabaseId = "e9027507-fca2-4ecd-b7b9-e8489fd992f3"
            },
            new DatabaseInteractionField
            {
                Id = "72fc282b-204e-419b-a5eb-f5ca06829032",
                DateTimeCreated = DateTime.UtcNow,
                Name = "SIGNOR Mechanism",
                Description = "The mechanism of an interaction in the SIGNOR database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "e9027507-fca2-4ecd-b7b9-e8489fd992f3"
            },
            new DatabaseInteractionField
            {
                Id = "6e61d143-0afe-4077-a975-a789bd49588c",
                DateTimeCreated = DateTime.UtcNow,
                Name = "SIGNOR Effect",
                Description = "The effect of an interaction in the SIGNOR database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "e9027507-fca2-4ecd-b7b9-e8489fd992f3"
            },
            new DatabaseInteractionField
            {
                Id = "a1708263-24a2-4bff-b264-e721110b9085",
                DateTimeCreated = DateTime.UtcNow,
                Name = "SIGNOR PubMed ID",
                Description = "The PubMed identifier of an interaction in the SIGNOR database.",
                IsSearchable = false,
                Url = "http://www.ncbi.nlm.nih.gov/pubmed/",
                DatabaseId = "e9027507-fca2-4ecd-b7b9-e8489fd992f3"
            },
            // The interaction fields in the STRING database.
            new DatabaseInteractionField
            {
                Id = "34c047b5-4d95-49f8-a98b-5ca9e1fb77c0",
                DateTimeCreated = DateTime.UtcNow,
                Name = "STRING Mode",
                Description = "The mode of an interaction in the STRING database.",
                IsSearchable = false,
                Url = null,
                DatabaseId = "52ac12fc-8a00-475e-9c61-23f4de7cb843"
            }
        };
    }
}
