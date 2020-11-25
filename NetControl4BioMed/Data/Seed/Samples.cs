using NetControl4BioMed.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetControl4BioMed.Data.Seed
{
    /// <summary>
    /// Represents the seed data for the samples.
    /// </summary>
    public static class Samples
    {
        /// <summary>
        /// Represents the database types to be seeded.
        /// </summary>
        public static ICollection<Sample> Seed { get; } = new List<Sample>
        {
            new Sample
            {
                Id = "01566f38-032c-4e22-a0f1-c6fcab1eecdd",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Tutorial generic network seed edges",
                Description = "These are the seed edges used by the generic network created in the tutorial They represent a randomly generated network with 10 nodes and 30% probability of creation for each edge.",
                Type = Enumerations.SampleType.SeedEdges,
                Data = "node1;node2\nnode1;node4\nnode1;node10\nnode2;node1\nnode2;node3\nnode2;node9\nnode3;node2\nnode3;node7\nnode3;node9\nnode5;node1\nnode5;node7\nnode5;node10\nnode6;node4\nnode7;node2\nnode7;node5\nnode7;node8\nnode7;node9\nnode8;node1\nnode8;node6\nnode8;node9\nnode9;node5\nnode9;node6\nnode10;node1\nnode10;node2\nnode10;node5\nnode10;node6\nnode10;node7\n"
            },
            new Sample
            {
                Id = "11ee7b05-2707-42d1-8c1e-486805f73fd3",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Tutorial generic analysis source nodes",
                Description = "These are the source nodes used by the generic analysis created in the tutorial. They represent a set of randomly chosen 7 nodes from the list of nodes in the tutorial generic network.",
                Type = Enumerations.SampleType.SourceNodes,
                Data = "node4\nnode8\nnode9"
            },
            new Sample
            {
                Id = "a8edc5f3-07a9-4ad6-95b1-f578de135742",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Tutorial generic analysis target nodes",
                Description = "These are the target nodes used by the generic analysis created in the tutorial. They represent a set of randomly chosen 3 nodes from the list of nodes in the tutorial generic network.",
                Type = Enumerations.SampleType.TargetNodes,
                Data = "node1\nnode4\nnode5\nnode6\nnode7\nnode8\nnode10\n"
            },
            new Sample
            {
                Id = "24f8e55a-2b26-45b6-8ec8-49a8c3a9cbc1",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Tutorial PPI network seed nodes",
                Description = "These are the seed nodes used by the PPI network created in the tutorial. They represent a set of randomly selected protein identifiers. They represent a set of randomly chosen 20 nodes from the list of nodes in the tutorial PPI network.",
                Type = Enumerations.SampleType.SeedNodes,
                Data = "EGFR\nYES1\nMAPK3\nACE2\nSOD1\nPPARG\nPTPN1\nSLC2A1\nFLT1\nERBB2"
            },
            new Sample
            {
                Id = "f51b9c3e-c1a7-4d27-bbc7-1d2b4d78bde7",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Tutorial PPI analysis source nodes",
                Description = "These are the source nodes used by the PPI analysis created in the tutorial. They represent a set of randomly chosen 15 nodes from the list of nodes in the tutorial PPI network.",
                Type = Enumerations.SampleType.SourceNodes,
                Data = "CDK1\nHDAC6\nRPS6KA3\nUSP8\nJUN\nFOXO1\nCSNK2A1\nANXA1\nGRB2\nPTPRR\nEGF\nPTPN5\nKIT\nPTK2\nSTAT3\n"
            },
            new Sample
            {
                Id = "b5cdb2fb-82ce-4753-b84a-5cd29e419755",
                DateTimeCreated = DateTime.UtcNow,
                Name = "Tutorial PPI analysis target nodes",
                Description = "These are the target nodes used by the PPI analysis created in the tutorial. They represent a set of randomly chosen 20 nodes from the list of nodes in the tutorial PPI network.",
                Type = Enumerations.SampleType.TargetNodes,
                Data = "ANXA1\nGRAP\nRHOA\nMAP2K3\nDAPK1\nHDAC6\nNTRK1\nJUN\nDUSP1\nRPS6KA1\nYES1\nPTPN2\nHSP90AA1\nNR2F2\nERBB2\nARRB1\nABL1\nJUNB\nPEA15\nSTAT3\n"
            }
        };
    }
}
