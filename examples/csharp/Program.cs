using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
public static class Program
{
    private static int assertions;
    private static void Check(bool condition, string name)
    {
        assertions++;
        if (!condition) throw new Exception("FAILED: " + name);
    }
    private static void Eq<T>(T actual, T expected, string name) => Check(EqualityComparer<T>.Default.Equals(actual,expected),name);
    private static void Seq<T>(IEnumerable<T> actual, IEnumerable<T> expected, string name) => Check(actual.SequenceEqual(expected),name);
    private static void Throws<T>(Action action, string name) where T:Exception
    {
        try { action(); } catch (T) { Check(true,name); return; }
        throw new Exception("FAILED (did not throw): " + name);
    }
    private static LinkedLists.Node? List(params int[] values)
    {
        LinkedLists.Node? head = null;
        for (int i = values.Length-1; i >= 0; i--) head = new LinkedLists.Node(values[i],head);
        return head;
    }
    private static int[] ListValues(LinkedLists.Node? head)
    {
        var answer = new List<int>();
        for (var node = head; node != null; node = node.Next)
        {
            if (answer.Count > 100) throw new Exception("Unexpected list cycle.");
            answer.Add(node.Value);
        }
        return answer.ToArray();
    }
    public static void Main()
    {
        Eq(Hashing.TwoSum(new[]{3,3},6),((int,int)?)(0,1),"two sum duplicate positions");
        Check(Hashing.TwoSum(new[]{3},6)==null,"two sum no self match");
        Check(Hashing.TwoSum(new[]{int.MinValue,0},int.MinValue)==(0,1),"two sum int minimum");
        Check(Hashing.TwoSum(new[]{int.MaxValue,1},int.MinValue)==null,"two sum overflow not a match");
        Check(Hashing.AreAnagrams("aab","aba"),"anagram yes"); Check(!Hashing.AreAnagrams("aab","abb"),"anagram multiplicity");
        Check(TwoPointers.SortedPair(new[]{1,3,7,9},10)==(0,3),"sorted pair"); Check(TwoPointers.SortedPair(Array.Empty<int>(),0)==null,"empty pair");
        Check(TwoPointers.IsPalindrome("") && TwoPointers.IsPalindrome("abba") && !TwoPointers.IsPalindrome("abc"),"palindrome");
        var chars = "abcd".ToCharArray(); TwoPointers.Reverse(chars); Eq(new string(chars),"dcba","reverse chars");
        int[] compact = {0,7,0,4}; Eq(TwoPointers.CompactNonzero(compact),2,"compact length"); Seq(compact.Take(2),new[]{7,4},"compact prefix");
        Eq(Windows.MaxFixedSum(new[]{-5,-2,-3},2),-5L,"negative fixed sum"); Throws<ArgumentOutOfRangeException>(()=>Windows.MaxFixedSum(new[]{1},0),"fixed invalid k");
        Eq(Windows.LongestAtMostKDistinct("eceba",2),3,"k distinct"); Eq(Windows.LongestAtMostKDistinct("abc",0),0,"zero distinct");
        Eq(Windows.MinPositiveLength(new[]{2,1,4,2},6),2,"shortest positive"); Eq(Windows.MinPositiveLength(new[]{1,2},8),0,"shortest absent");
        Throws<ArgumentException>(()=>Windows.MinPositiveLength(new[]{1,-1},1),"reject negative sliding window");
        Eq(PrefixSums.CountTargetSum(new[]{1,-1,1},1),3L,"prefix negatives"); Eq(PrefixSums.CountTargetSum(new[]{0,0,0},0),6L,"prefix multiplicity");
        Seq(PrefixSums.Build(new[]{2,-1,5}),new long[]{0,2,1,6},"exclusive prefix");
        Check(!LinkedLists.HasCycle(null),"null cycle"); var cycle = List(1,2,3)!; cycle.Next!.Next!.Next = cycle.Next; Check(LinkedLists.HasCycle(cycle),"list cycle");
        Eq(LinkedLists.Middle(List(1,2,3,4))!.Value,3,"second middle"); Seq(ListValues(LinkedLists.Reverse(List(1,2,3))),new[]{3,2,1},"reverse list");
        Seq(ListValues(LinkedLists.Merge(List(1,4),List(2,3))),new[]{1,2,3,4},"merge lists");
        Seq(ListValues(LinkedLists.ReverseRange(List(1,2,3,4),1,3)),new[]{3,2,1,4},"reverse head range");
        Seq(Stacks.NextGreaterValues(new[]{2,2,3}),new[]{3,3,-1},"strict next greater");
        Check(Stacks.ValidBrackets("([])"),"brackets true"); Check(!Stacks.ValidBrackets("([)]") && !Stacks.ValidBrackets("a"),"brackets false");
        Eq(Stacks.LargestRectangle(new[]{2,1,5,6,2,3}),10L,"histogram"); Eq(Stacks.LargestRectangle(Array.Empty<int>()),0L,"histogram empty");
        Eq(Stacks.TrappedWater(new[]{3,0,2,0,4}),7L,"trapped water"); Eq(Stacks.TrappedWater(new[]{1}),0L,"water singleton");
        Eq(Heaps.KthLargest(new[]{4,1,9,7},2),7,"kth largest"); Throws<ArgumentOutOfRangeException>(()=>Heaps.KthLargest(new[]{1},2),"kth invalid");
        Seq(Heaps.MergeSortedArrays(new[]{new[]{1,4},Array.Empty<int>(),new[]{2,3}}),new[]{1,2,3,4},"kway merge");
        Seq(Heaps.TopFrequent(new[]{1,1,1,2,2,3},2).Order(),new[]{1,2},"frequencies");
        var median = new Heaps.RunningMedian(); Throws<InvalidOperationException>(()=>median.Median(),"empty median");
        median.Add(int.MinValue); median.Add(int.MaxValue); Eq(median.Median(),-0.5,"median extremes"); median.Add(0); Eq(median.Median(),0.0,"odd median");
        Eq(BinarySearch.Exact(Array.Empty<int>(),5),-1,"empty exact"); Eq(BinarySearch.Exact(new[]{2,4,6},4),1,"exact index");
        Eq(BinarySearch.LowerBound(new[]{2,2,5},2),0,"lower duplicate"); Eq(BinarySearch.LowerBound(new[]{2,2,5},9),3,"lower end");
        Eq(BinarySearch.FirstTrue(10,i=>i>=7),7,"first true"); Eq(BinarySearch.FirstTrue(0,_=>throw new Exception()),0,"no predicate outside range");
        Eq(BinarySearch.FirstTrue(3,_=>false),3,"all false"); Eq(BinarySearch.Rotated(new[]{4,5,6,1,2,3},2),4,"rotated");
        Eq(BinarySearch.MinimumShippingCapacity(new[]{3,2,2,4},2),6L,"ship capacity"); Eq(BinarySearch.MinimumShippingCapacity(new[]{int.MaxValue,int.MaxValue},1),4294967294L,"ship overflow safety");
        Seq(Intervals.Merge(new[]{(1,3),(3,5),(8,9)}),new[]{(1,5),(8,9)},"closed merge touching");
        Eq(Intervals.MeetingRooms(new[]{(1,3),(2,4),(3,5)}),2,"room endpoint tie"); Eq(Intervals.MaxNonOverlapping(new[]{(1,3),(2,4),(3,5)}),2,"greedy intervals");
        int[][] grid = {new[]{0,0},new[]{1,0}};
        Eq(GridTraversal.ShortestPath(grid,(0,0),(1,1)),2,"BFS edges"); Eq(GridTraversal.ShortestPath(grid,(0,0),(0,0)),0,"BFS same source");
        Eq(GridTraversal.ShortestPath(grid,(1,0),(1,1)),-1,"BFS blocked source"); Eq(GridTraversal.ShortestPath(Array.Empty<int[]>(),(0,0),(0,0)),-1,"BFS empty");
        Eq(GridTraversal.ShortestPath(new[]{new[]{0,1,0}},(0,0),(0,2)),-1,"BFS unreachable");
        Throws<ArgumentException>(()=>GridTraversal.ShortestPath(new[]{new[]{0},new[]{0,0}},(0,0),(1,0)),"BFS ragged reject");
        Eq(GridTraversal.Islands(new[]{"110".ToCharArray(),"010".ToCharArray(),"001".ToCharArray()}),2,"islands");
        var land = new[]{"11".ToCharArray(),"01".ToCharArray()}; GridTraversal.RecursiveFloodFill(land,0,0); Eq(new string(land[0])+new string(land[1]),"0000","recursive fill");
        Eq(GridTraversal.RottingMinutes(new[]{new[]{2,1,1,2}}),1,"multisource"); Eq(GridTraversal.RottingMinutes(new[]{new[]{1}}),-1,"unreachable fresh"); Eq(GridTraversal.RottingMinutes(new[]{new[]{0}}),0,"no fresh");
        Seq(Graphs.TopologicalOrder(3,new[]{(0,2),(1,2)})!,new[]{0,1,2},"topo ordering"); Check(Graphs.TopologicalOrder(2,new[]{(0,1),(1,0)})==null,"topo cycle");
        Eq(Graphs.TopologicalOrder(3,Array.Empty<(int,int)>())!.Length,3,"topo isolated"); Eq(Graphs.Components(4,new[]{(0,1)}),3,"components isolated");
        var distance = Graphs.Dijkstra(4,new[]{(0,1,8),(0,2,2),(2,1,1)},0); Seq(distance,new[]{0L,3L,2L,long.MaxValue},"Dijkstra stale and disconnected");
        Eq(Graphs.Dijkstra(3,new[]{(0,1,int.MaxValue),(1,2,int.MaxValue)},0)[2],4294967294L,"Dijkstra long distance");
        Throws<ArgumentException>(()=>Graphs.Dijkstra(2,new[]{(0,1,-1)},0),"Dijkstra negative reject");
        var uf = new UnionFind(4); Check(uf.Union(0,1) && uf.Union(2,3) && uf.Union(1,2) && !uf.Union(0,3),"union-find duplicate union"); Eq(uf.Components,1,"component count");
        Eq(Backtracking.Subsets(new[]{1,2}).Count,4,"subsets count"); Seq(Backtracking.Subsets(Array.Empty<int>())[0],Array.Empty<int>(),"empty subset");
        var subsets = Backtracking.Subsets(new[]{1,2}); Check(!ReferenceEquals(subsets[1],subsets[2]),"snapshot nonaliasing");
        Seq(Backtracking.Parentheses(2),new[]{"(())","()()"},"parentheses"); Eq(Backtracking.Parentheses(0)[0],"","empty parentheses");
        var board = new[]{"AB".ToCharArray(),"CD".ToCharArray()}; Check(Backtracking.WordExists(board,"ABD"),"word found"); Check(!Backtracking.WordExists(board,"ABA"),"word cell reuse prohibited"); Eq(new string(board[0]),"AB","board preserved");
        var trie = new Trie(); trie.Insert("car"); trie.Insert("cart"); Check(trie.Search("car") && trie.StartsWith("ca") && !trie.Search("ca"),"trie prefix versus word");
        trie.Insert(""); Check(trie.Search(""),"trie empty word"); Throws<ArgumentException>(()=>trie.Insert("Bad"),"trie alphabet");
        Eq(DynamicProgramming.StairWays(0),BigInteger.One,"stairs zero"); Eq(DynamicProgramming.StairWays(3),new BigInteger(3),"stairs count");
        Eq(DynamicProgramming.Rob(new[]{2,7,9,3,1}),12L,"robber"); Eq(DynamicProgramming.Rob(new[]{-1,-2}),0L,"robber optional empty");
        Eq(DynamicProgramming.MinStairCost(new[]{10,15,20}),15L,"stair cost"); Throws<ArgumentException>(()=>DynamicProgramming.MinStairCost(new[]{-5}),"negative stair cost reject");
        Eq(DynamicProgramming.FewestCoins(new[]{1,3,4},6),2,"coin greedy counterexample"); Eq(DynamicProgramming.FewestCoins(new[]{2},3),-1,"coin impossible");
        Check(!DynamicProgramming.SubsetSumOnce(new[]{3},6),"zero one no reuse"); Check(DynamicProgramming.SubsetSumOnce(new[]{3,3},6),"distinct positions once");
        Eq(DynamicProgramming.LisLength(new[]{3,1,2,2}),2,"strict LIS duplicates"); Eq(DynamicProgramming.EditDistance("kitten","sitting"),3,"edit distance");
        Eq(DynamicProgramming.EditDistance("","abc"),3,"edit empty base"); Eq(DynamicProgramming.MemoizedLcs("abcde","ace"),3,"memoized LCS"); Eq(DynamicProgramming.UniquePaths(2,3),new BigInteger(3),"grid DP");
        var tree = new Trees.Node(5,new Trees.Node(2),new Trees.Node(8)); Eq(Trees.Depth(tree),2,"tree depth"); Seq(Trees.Levels(tree)[1],new[]{2,8},"tree level");
        Check(Trees.ValidBst(tree),"BST true"); Check(!Trees.ValidBst(new Trees.Node(10,new Trees.Node(5,null,new Trees.Node(12)))),"BST inherited bound");
        Check(Trees.ValidBst(new Trees.Node(int.MinValue,null,new Trees.Node(int.MaxValue))),"BST extreme keys"); Eq(Trees.KthSmallest(tree,2),5,"BST kth");
        Check(ReferenceEquals(Trees.LowestCommonAncestor(tree,tree.Left!,tree.Right!),tree),"LCA node references");
        Eq(Trees.Serialize(Trees.Deserialize(Trees.Serialize(tree))),Trees.Serialize(tree),"tree roundtrip"); Eq(Trees.Serialize(null),"#","null tree encoding");
        Throws<FormatException>(()=>Trees.Deserialize("1,#"),"tree truncated"); Throws<FormatException>(()=>Trees.Deserialize("#,#"),"tree trailing");
        Seq(Matrices.Spiral(new[]{new[]{1,2,3}}),new[]{1,2,3},"spiral single row"); Seq(Matrices.Spiral(new[]{new[]{1},new[]{2},new[]{3}}),new[]{1,2,3},"spiral single column");
        int[][] square = {new[]{1,2},new[]{3,4}}; Matrices.RotateClockwise(square); Seq(square.SelectMany(x=>x),new[]{3,1,4,2},"rotate clockwise");
        int[][] zeroes = {new[]{1,2,3},new[]{4,0,6},new[]{7,8,9}}; Matrices.SetZeroes(zeroes); Seq(zeroes.SelectMany(x=>x),new[]{1,0,3,0,0,0,7,0,9},"matrix zero markers");
        Eq(ArrayTricks.Duplicate(new[]{1,3,4,2,2}),2,"array cycle duplicate"); Eq(ArrayTricks.Majority(new[]{2,2,1}),(int?)2,"majority"); Check(ArrayTricks.Majority(new[]{1,2})==null,"majority verify");
        Seq(ArrayTricks.ProductExceptSelf(new[]{2,0,4}),new long[]{0,8,0},"product zero"); Seq(ArrayTricks.ProductExceptSelf(new[]{7}),new long[]{1},"product singleton");
        Eq(ArrayTricks.SingleByXor(new[]{9,2,9}),2,"xor cancellation"); Eq(ArrayTricks.SetBitCount(12),2,"clear bit"); Eq(ArrayTricks.SetBitCount(uint.MaxValue),32,"all bits");
        var cache = new Caches.Lru(2); cache.Put(1,10); cache.Put(2,20); Check(cache.TryGet(1,out int ten) && ten==10,"cache hit"); cache.Put(3,30); Check(!cache.TryGet(2,out _),"cache eviction");
        var noCache = new Caches.Lru(0); noCache.Put(1,1); Check(!noCache.TryGet(1,out _),"cache zero capacity");
        var minStack = new Caches.MinStack(); minStack.Push(2); minStack.Push(1); minStack.Push(1); minStack.Pop(); Eq(minStack.Minimum(),1,"duplicate min retained"); minStack.Pop(); Eq(minStack.Minimum(),2,"minimum restored");
        Eq(Greedy.BestSingleTrade(new[]{7,1,5,3,6,4}),5L,"single trade"); Eq(Greedy.MaximumSubarray(new[]{-4,-2,-7}),-2L,"all negative Kadane");
        Check(Greedy.CanReachEnd(new[]{2,3,1,1,4}) && !Greedy.CanReachEnd(new[]{3,2,1,0,4}),"jump frontier");
        Eq(CollectionApis.MinHeapRoot(),1,"minheap API"); Eq(CollectionApis.MaxHeapRoot(new[]{int.MinValue,int.MaxValue}),int.MaxValue,"maxheap API extremes");
        Eq(CollectionApis.Frequencies("aba")['a'],2,"frequency API"); Eq(CollectionApis.QueueOrder(),20,"queue API"); Eq(CollectionApis.StackOrder(),10,"stack API"); Eq(CollectionApis.DequeOrder(),2,"deque API");
        Eq(CollectionApis.DistinctCount(new[]{1,1,2}),2,"set API"); var counts = new Dictionary<string,int>{{"a",0},{"b",2}}; CollectionApis.RemoveNonpositive(counts); Check(!counts.ContainsKey("a") && counts["b"]==2,"snapshot mutation");
        Seq(CollectionApis.OrderedUnique(new[]{3,1,3}),new[]{1,3},"sortedset API"); Seq(CollectionApis.OrderedMap().Keys,new[]{"alpha","beta"},"sortedmap API");
        Seq(ArrayStringApis.Descending(new[]{1,3,2}),new[]{3,2,1},"descending API"); Eq(ArrayStringApis.Repeat('x',3),"xxx","builder API");
        Eq(ArrayStringApis.LowercaseIndex('z'),25,"lowercase index"); Eq(ArrayStringApis.LowercaseCharacter(0),'a',"lowercase inverse");
        Eq(ArrayStringApis.FindOrInsertionPoint(new[]{2,6},4),1,"binarysearch complement"); var allocation = ArrayStringApis.AllocateGrid(2,2); allocation[0][0]=true; Check(!allocation[1][0],"distinct row arrays");
        Seq(ArrayStringApis.Filled(3,-1),new[]{-1,-1,-1},"array fill"); Seq(ArrayStringApis.SortCoordinates(new[]{(1,0),(0,2),(0,1)}),new[]{(0,1),(0,2),(1,0)},"tuple lexicographic");
        Eq(ArrayStringApis.JoinIntegers(new[]{1,2}),"1,2","join"); Eq(ArrayStringApis.ReverseCopy("abc"),"cba","char copy");
        Eq(Numerics.Midpoint(int.MinValue,int.MaxValue),-1,"full int midpoint"); Eq(Numerics.Sum(new[]{int.MaxValue,int.MaxValue}),4294967294L,"sum wide");
        Eq(Numerics.Product(100000,100000),10000000000L,"multiply widening"); Throws<OverflowException>(()=>Numerics.CheckedIncrement(int.MaxValue),"checked overflow"); Eq(Numerics.AddFiniteDistance(long.MaxValue,1),long.MaxValue,"sentinel guard");
        Seq(Stacks.DaysUntilWarmer(new[]{73,74,75,71,69,72,76,73}),new[]{1,1,4,2,1,1,0,0},"temperature distances");
        Seq(Stacks.StockSpans(new[]{100,80,60,70,60,75,85}),new[]{1,1,1,2,1,4,6},"stock spans");
        Seq(Heaps.KSmallest(new[]{4,1,9,7},2).Order(),new[]{1,4},"k smallest");
        Eq(BinarySearch.MinimumEatingSpeed(new[]{3,6,7,11},8),4,"eating speed");
        Eq(BinarySearch.MinimumLargestSplit(new[]{7,2,5,10,8},2),18L,"split array");
        Eq(Backtracking.Permutations(new[]{1,2,3}).Count,6,"permutations"); Eq(Backtracking.Combinations(new[]{1,2,3},2).Count,3,"combinations");
        Check(DynamicProgramming.IsInterleaving("ab","cd","acbd") && !DynamicProgramming.IsInterleaving("ab","cd","adbc"),"interleaving");
        Eq(DynamicProgramming.MinimumPathSum(new[]{new[]{1,3,1},new[]{1,5,1},new[]{4,2,1}}),7L,"minimum path sum");
        Check(ReferenceEquals(Trees.BstLowestCommonAncestor(tree,2,8),tree),"BST LCA");
        Seq(Trees.RightSideView(tree),new[]{5,8},"right view"); Seq(Trees.ZigzagLevels(tree)[1],new[]{8,2},"zigzag");
        Seq(ListValues(LinkedLists.MergeMany(new[]{List(1,4),List(2,3),null})),new[]{1,2,3,4},"merge many linked lists");
        StackLessonChecks();
        RandomizedChecks();
        Console.WriteLine($"PASS: {assertions} assertions; all reference source files compiled.");
    }
    private static void StackLessonChecks()
    {
        var originalOutput = Console.Out;
        using var captured = new System.IO.StringWriter();
        try
        {
            Console.SetOut(captured);
            StackBasics.Demonstrate();
        }
        finally
        {
            Console.SetOut(originalOutput);
        }
        string expected = string.Join(Environment.NewLine, new[] {
            "Top: 20", "Count: 2", "Removed: 20", "Now top: 10",
            "Removed: 10", "The stack is empty."
        }) + Environment.NewLine;
        Eq(captured.ToString(), expected, "stack lesson exact operations trace");

        foreach (string text in new[] { "", "()", "([])", "{[()]}", "()[]{}" })
            Check(StackBrackets.IsBalanced(text), "stack lesson valid brackets: " + text);
        foreach (string text in new[] { "]", "(", "(()", "([)]", "(]", "())", "a", "(a)", " " })
            Check(!StackBrackets.IsBalanced(text), "stack lesson invalid brackets: " + text);
        Throws<ArgumentNullException>(() => StackBrackets.IsBalanced(null!), "stack lesson null brackets");

        int[] example = { 4, 2, 3, 5 };
        Seq(StackNextGreater.Values(example), new[] { 5, 3, 5, -1 }, "stack lesson worked next-greater trace");
        Seq(example, new[] { 4, 2, 3, 5 }, "stack lesson does not mutate input");
        Seq(StackNextGreater.Values(Array.Empty<int>()), Array.Empty<int>(), "stack lesson empty next greater");
        Seq(StackNextGreater.Values(new[] { 2, 2, 3 }), new[] { 3, 3, -1 }, "stack lesson strict duplicate handling");
        Seq(StackNextGreater.Values(new[] { 3, 3 }), new[] { -1, -1 }, "stack lesson equal is not greater");
        Seq(StackNextGreater.Values(new[] { 5, 4, 3, 2 }), new[] { -1, -1, -1, -1 }, "stack lesson all positions wait");
        Seq(StackNextGreater.Values(new[] { -2, -1 }), new[] { -1, -1 }, "stack lesson documented sentinel ambiguity");
        Seq(StackNextGreater.Values(new[] { int.MinValue, int.MaxValue }), new[] { int.MaxValue, -1 }, "stack lesson extreme values");
        Throws<ArgumentNullException>(() => StackNextGreater.Values(null!), "stack lesson null next greater");
    }
    private static void RandomizedChecks()
    {
        var random = new Random(1701);
        for (int round = 0; round < 200; round++)
        {
            int[] values = Enumerable.Range(0,random.Next(0,14)).Select(_=>random.Next(-5,6)).ToArray();
            int target = random.Next(-8,9); long count = 0; bool pair = false;
            for (int i = 0; i < values.Length; i++)
            {
                long sum = 0;
                for (int j = i; j < values.Length; j++) { sum += values[j]; if (sum==target) count++; }
                for (int j = i+1; j < values.Length; j++) if ((long)values[i]+values[j]==target) pair=true;
            }
            Eq(PrefixSums.CountTargetSum(values,target),count,"random prefix count"); Eq(Hashing.TwoSum(values,target)!=null,pair,"random pair existence");
            int[] sorted=values.Order().ToArray(); int lower=Array.FindIndex(sorted,x=>x>=target); if(lower<0) lower=sorted.Length;
            Eq(BinarySearch.LowerBound(sorted,target),lower,"random lower bound");
            int[] next=new int[values.Length]; Array.Fill(next,-1);
            for(int i=0;i<values.Length;i++) for(int j=i+1;j<values.Length;j++) if(values[j]>values[i]) {next[i]=values[j];break;}
            Seq(Stacks.NextGreaterValues(values),next,"random next greater");
            Seq(StackNextGreater.Values(values),next,"random stack lesson next greater");
            if(values.Length>0)
            {
                long best=long.MinValue;
                for(int i=0;i<values.Length;i++){long sum=0;for(int j=i;j<values.Length;j++){sum+=values[j];best=Math.Max(best,sum);}}
                Eq(Greedy.MaximumSubarray(values),best,"random Kadane");
                int k=random.Next(1,values.Length+1); Eq(Heaps.KthLargest(values,k),sorted[^k],"random kth");
            }
        }
    }
}
