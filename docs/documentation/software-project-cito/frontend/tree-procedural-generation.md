# Tree procedural generation

## Context
The trees provided by Citolab used by the app need to be visualised. There are two aspects to this; 

- The visualisation and styling of the graph
- Procedurally generating the graph

This document will primarily focus on the second aspect.

The app makes use of complex algorithm to generate the graph structure. Naturally, there are endless amounts of ways to do this. To determine which visualisation is better than others, we look at the following criteria:

#### 1. Clarity

The visualisation needs to be clear. It should be easy to identify each nodes' predecessors and successors at a glance, without having to make guesses.

Means to achieve this are e.g. removing edge crossings. 
> **NOTE**  
> Edge crossings cannot always be eliminated due to directional constraints.

An example of such a situation is the following;

```
1-----3    
  \ /   
   X    
  / \  
2-----4
```

Where we have edges (1,3), (1,4), (2,3), (2,4). The critical thinker may see a solution, by moving node 2 to the right of nodes 3 and 4. However, this would contradict the directionality of the graph.

If this issue occurs, it is best to discuss with Citolab what kind of solution they prefer to this issue.

#### 2. Domain expansion

The visualisation **must** support collapsing and expanding of domains, meaning that parts of the tree will have to be hidden on command. Citolab consider making it such that only one domain may be expanded at a time, though this is currently an AB-tested feature. 

> **IMPORTANT**     
> Expanding or collapsing domains must **never change the underlying graph layout**. Only node visibility changes. Computing the tree with a different node set will greatly alter the layout, leading to confusion.

#### 3. Metro-like

The visualisation should look akin to a real life transit / Mini Metro style map. (Mini Metro is a game on Steam which you should give a close look for inspiration) This criteria is rather subjective, since there is no measure of 'Mini Metroness'. This can be done both through the structure of the visualisation, but should primarily be done through styling of the graph.

## Implementation

In order to handle domain expansion, we introduced two types of nodes: "domain" and topic nodes, which have type "locked", "unlocked" or "mastered". These have to be carefully handled separately. A domain node should replace all the topic nodes from that topic.

The procedural generation can roughly be divided into two parts.

1. Computing the 'default' graph    (computeRelativeLayout.tsx)
2. Computing the actual graph       (computeAbsoluteLayout.tsx)

The 'default' graph being the graph when all domains are expanded. As mentioned in (see: Context), the underlying graph should not change when collapsing or expanding a domain. We can therefore do the relatively expensive procedural generation of the graph once, and then compute the actual layout repeatedly given which domains are expanded.

> **IMPORTANT NOTE**  
> Currently, on mobile, the tree visualisation is hardcoded. This should become modular as well, preferably using the same algorithm. Because of the standard mobile screen dimensions, the idea was to transpose the tree and make it go from top to bottom, rather than the left to right layout on other devices. Unfortunately we ran out of time before implementing this. Importantly, any changes to the tree will break the mobile version, so please keep this in mind in the future.

## 1. Computing the 'default' graph

Given the nodes and edges, it returns the subgraph belonging to each domain and for each domain, the width it spans.

This can step can further be divided into two parts:

### 1.1. Computing the full graph

This is the most important and subjective part. Given the nodes and edges, return a positioning for each node. The algorithm can be described by the following steps:

#### 1.1.1. Decompose the graph

The algorithm starts off by finding a 'main chain'. This chain is the longest possible chain, and in the case of multiple options, the first one it finds. We can now distinguish all the other paths into three distinct categories:

> **Type A: 'Ear' branches**  
> These are paths which start and end on the main chain. Formally, they have the form    
> $(u, n_1, ..., n_k, v)$      
> where $k >= 1$, $u$ and $v$ are nodes in the main chain, but not necessarily adjacent. 

> **Type B: 'Outgoing' branches**  
> These are paths which end on the main chain. Formally, they have the form     
> $(u, n_1, ..., n_k)$  
> where $k >= 1$, $u$ is a node in the main chain.

> **Type C: 'Incoming' branches**    
> These are paths which start on the main chain. Formally, they have the form   
> $(n_1, ..., n_k, v)$  
> where $k >= 1$, $v$ is a node in the main chain.

Note that Citolab mentioned that the data will always have a single starting node. We can therefore **never** have paths of type C. A quick informal proof outline of this fact (Proof Outline 1) is found at the end of the document. We therefore only consider branches of type A and B and identify them.

#### 1.1.2. Calculate the topological depths

The topological depth of a node $N$ is defined as

> the length of the shortest path from the starting node to $N$

We thus start at the starting node and perform a BFS to find each nodes' topological depth.

#### 1.1.3. Assign node ordering

Now, the algorithm does a path-based assignment to a grid map. It initialises the main chain as the base, where each main chain node is added at the root of its respective depth; node $i$ of the main chain gets position 1 at depth $i$.

Then, the algorithm traverses the ear branches. Since these re-join with the main chain eventually, unlike outgoing branches, they may produce edge crossings. Hence, to minimise / eliminate edge crossings, we consider these first. It places the entire ear branch at the lowest height possible without colliding with other nodes. Afterwards, the same happens, but now for the outgoing branches.

Note that this current approach does **not** guarantee the absence of edge crossings on graphs which are *planar*.

> A planar graph is a graph which can be represented in a plane without edge crossings.

This would be something for a v2 implementation, if Citolab desires so. Consider an approach like simulated annealing or something else creative.

#### 1.1.4. Grid snap

Using the grid map which we just created, we derive the exact coordinates to be used. The algorithm then snaps each coordinate to a 50-unit grid, which currently does nothing, but might be useful later. Consider adding neat improvement algorithms in between derivation and snapping.

### 1.2. Determining domain widths and making positions relative

Given the positioning of each node, determine how wide each domain spans and move the nodes relative to the start of the domain. 

The algorithm does this by traversing all the nodes and finding the maximum and minimum X values for each domain. Then, each node is shifted to the left based on how wide the previous domains are.

## 2. Computing the actual graph

We now derive the absolute positions of the nodes to be used by the visualisation. This can be divided into the following steps

### 2.1. Compute domain offset

Using the widths of all the domains *in combination with* which domains are expanded, we determine the offset which each domain has from the starting node. A collapsed domain has a default width rather than its own domain, because it is represented as a single domain node, which also keeps the following domains in line.

### 2.2. Compute absolute positions

We then simply iterate over each node and add its respective domain offset to its positional value. Note that the interface provided by the tree component also requires a list of all visible ids, which we then also calculate and send, alongside the absolute node positions.

## Future improvements

The following parts should be improved in future iterations:

- Implement mobile version
- Add a domain property to nodes (see utils/tree/getNodeDomain.tsx)
- Add separate properties for domain/topic node and its status (locked/unlocked/mastered)
- Import domain order rather than hardcoding it
- Increase 'Mini Metroness'


## Appendix - Proof outline 1

Assume towards a contradiction that we do have a path $P=(n_1, ..., n_k, v)$, with $v$ on the main chain. Let $v$ be the $i$-th node of the main chain $M=(m_1, ..., m_{i-1}, v, m_{i+1}, ..., m_j)$ with $|M| = j$. We can distinguish two cases:

- Case 1: $k <= i$  
The main chain is correctly identified as the longest possible path. However, the existance of $P$ contradicts our assumption that we have a single starting node.

- Case 2: $k > i$   
The main chain is actually shorter than the path formed by combining $P$ and $M$; $M' = (n_1, ..., n_k, v, m_{i+1}, ..., m_j)$, which is a contradiction.

---