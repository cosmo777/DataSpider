# DataSpider
Tool for crawling data structures

Data Spider is a simple tool used for looking for values in a memory structure when you have a starting point.

I have been using it for finding offsets to new values when I have a starting point.

There is a samples folder with an example of its use.

In the sample I am looking for the offsets to get to the name of an npc in the game Path of Exile.

I have the NPC's pointers and the values of their names but the path from the base NPC pointer to the name is unknown.

I load each of the NPC's pointers in a tab in the program and search for their name, afterwards I hit the Matches button below the tabs and I only see the offsets that were the same for each NPC to get to their name. 
