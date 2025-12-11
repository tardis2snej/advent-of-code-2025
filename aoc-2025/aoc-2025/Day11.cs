namespace aoc_2025;

public class Day11
{
	private class Node
	{
		private static readonly Dictionary<string, Node> NodesLookup = new();
		
		public string Name;
		public bool IsVisited;
		public bool IsDeadEnd;
		public Node[] Connections = [];

		public static Node Init(string name, string[] connections = null)
		{
			if (!NodesLookup.TryGetValue(name, out Node node))
			{
				node = new Node(name);
				NodesLookup.Add(name, node);
			}

			if (connections != null)
			{
				node.UpdateConnections(connections);
			}

			return node;
		}

		public static void ResetAll()
		{
			foreach (var node in NodesLookup.Values)
			{
				node.Reset();
			}
		}
		
		public static Node Find(string name) => NodesLookup.GetValueOrDefault(name);
		
		private Node(string name)
		{
			Name = name;
		}

		private void UpdateConnections(string[] connections)
		{
			Node[] nodes = new Node[connections.Length];
			for (int i = 0; i < connections.Length; i++)
			{
				if (NodesLookup.TryGetValue(connections[i], out var node))
				{
					nodes[i] = node;
				}
				else
				{
					nodes[i] = Node.Init(connections[i]);
				}
			}

			Connections = nodes;
		}

		private void Reset()
		{
			IsVisited = false;
			IsDeadEnd = false;
		}
	}

	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

		for (int i = 0; i < lines.Length; i++)
		{ 
			string[] nodesNames = lines[i].Split(' ');
			Node.Init(nodesNames[0].TrimEnd(':'), nodesNames[1..]);
		}
		
		string headName = "svr";
		string outName = "out";
		string[] keyNodes = ["fft", "dac"];

		long pathBetweenKeyNodes = CountPathRecursive(Node.Find(keyNodes[0]), keyNodes[1]);

		if (pathBetweenKeyNodes == 0)
		{
			Node.ResetAll();
			pathBetweenKeyNodes = CountPathRecursive(Node.Find(keyNodes[0]), keyNodes[1]);

			if (pathBetweenKeyNodes == 0)
			{
				Console.WriteLine("Error: key nodes aren't connected");
			}

			(keyNodes[0], keyNodes[1]) = (keyNodes[1], keyNodes[0]);
		}
		Console.WriteLine($"Key nodes paths {pathBetweenKeyNodes}");
		
		Node.ResetAll();
		long pathBetweenStartAndNode = CountPathRecursive(Node.Find(headName), keyNodes[0]);
		Console.WriteLine($"Start - Key nodes paths {pathBetweenStartAndNode}");
		
		Node.ResetAll();
		long pathBetweenNodeAndEnd = CountPathRecursive(Node.Find(keyNodes[1]), outName);
		Console.WriteLine($"Key nodes - end paths {pathBetweenNodeAndEnd}");
		
		Console.WriteLine( $"Total path count: {pathBetweenKeyNodes * pathBetweenStartAndNode * pathBetweenNodeAndEnd}");
	}

	private long CountPathRecursive(Node head, string targetName)
	{
		if (head.IsVisited)
		{
			return 0;
		}
		
		if (head.Name == targetName)
		{
			return 1;
		}

		long counter = 0;
	
		head.IsVisited = true;
		int childCount = head.Connections.Length;
		for(int i = 0; i < childCount; i++)
		{
			if (head.Connections[i].IsDeadEnd)
			{
				continue;
			}
			
			long pathCount = CountPathRecursive(head.Connections[i], targetName);
			head.Connections[i].IsDeadEnd = pathCount == 0;

			counter += pathCount;
		}

		head.IsVisited = false;

		return counter;
	}
}