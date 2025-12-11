namespace aoc_2025;

public class Day11
{
	public const string HEAD_NODE = "you";
	public const string TARGET_NODE = "out";

	private class Node
	{
		private static readonly Dictionary<string, Node> NodesLookup = new();
		
		public string Name;
		public bool IsVisited;
		public Node[] Connections;

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
	}

	public void Run(string input)
	{
		string text = File.ReadAllText(input);
		string[] lines = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

		Node[] nodes = new Node[lines.Length];
		Node head = null;
		for (int i = 0; i < lines.Length; i++)
		{ 
			string[] nodesNames = lines[i].Split(' ');
			nodes[i] = Node.Init(nodesNames[0].TrimEnd(':'), nodesNames[1..]);

			if (nodes[i].Name == HEAD_NODE)
			{
				head = nodes[i];
			}
		}

		if (head != null)
		{
			int counter = CountPathRecursive(head, TARGET_NODE);
			Console.WriteLine( $"Total path count: {counter}");
		}
	}

	private int CountPathRecursive(Node head, string targetName)
	{
		if (head.Name == targetName)
		{
			return 1;
		}

		head.IsVisited = true;
		
		Queue<Node> queue = new(head.Connections);

		int counter = 0;
		while (queue.TryDequeue(out Node child))
		{
			counter += CountPathRecursive(child, targetName);
		}

		head.IsVisited = false;

		return counter;
	}
}