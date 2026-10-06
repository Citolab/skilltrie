<p style="color:red; font-size:30px"><b>This document was originally a brainstorm for creating an extendable badge architecture. The details are outdated, but the idea of the flow is still accurate and is builded from the ground up, which can help with understanding the architecture. That is why we decided to include this document anyways. Some outdated elements have been pinpointed, but possibly not all.</b></p>


**Possible Solution**

First let’s define the tables

**The Badge Table:**  
*Identifier | Name | Description | Disabled | ProgressNeeded | OpenFrom | OpenUntil | TriggerCondition | Category?? | …other metadata*  <span style="color: red">(outdated: columns are shuffled over different tables)</span>
……………………………………………………………………………………………….. rows

This table represents the badges themselves, uniquely identified by the *ID* column and storing other useful information about the badge like until when the badge can be retrieved.

**The BadgeTriggers Table:**  
*BadgeID \[Int\] | TriggerCondition (GameEvent) \[Int\]*

Links a badge with its trigger condition(s), TriggerCondition is an enumerator stored as an Int

Example:  
1 | 1  
1 | 2  
2 | 1  
3 | 2

**The BadgeStudent Table:**  
*BadgeID | StudentId | Progress | DateAccomplished*  <span style="color: red">(outdated: DateAccomplished is moved to a different table)</span>
……………………………………………………………….. rows

This table represents which student has what kind of progress on which badge. So if a student with id 7 has accomplished a badge named “LOOL” on 26/05/2026, there will be a row:   
“LOOL”, 7, 1.0d, 26/05/2026  
The *Progress* column is an int and keeps track of how far the student is with the badge. When the Progress value is equal to Badge.ProgressNeeded, the student has accomplished the badge. This way we can keep track of how many items a student has completed for example. For badges which have no progress like whether you have mastered a topic, the *Progress* column will act as a boolean and will always be 0 until you complete it.

Finally, we have an interface called IBadgeHandler which is defined the following

<span style="color: red">(outdated: parameters of calculateNewProgress has changed)</span>
```csharp
interface IBadgeHandler {  
	public string badgeIdentifier();  
	public int calculateNewProgress(BadgeInputData data);  
}
```

For each row in the **Badge** table there will be a class implementing **IBadgeHandler**. So we get a one to one mapping of rows and **IBadgeHandler** implementations.

Now we can talk about how the acquiring of badges will work.  
First of all we will trigger **GameEvent** enum instances over different pivotal places in the backend. For example **GameEvent.TestCompleted** when a test is completed, or **GameEvent.ItemSubmitted** for when a user submits an item, or **GameEvent.StreakProlonged** for when, fill in the blanks. The **GameEvent** enum is also the type of *TriggerCondition* in the **Badge** table.

We make an additional class called something like **BadgeGameEventHandler**. This class is subscribed to the **GameEvent** being triggered. It will handle the event the following way. It will fetch all the records in **Badge** for which *TriggerCondition* \== GameEvent. Now for each **Badge** it can find the corresponding **IBadgeHandler** instances by the *Name* attribute from a list of all the instances of **IBadgeHandler** which had been collected on startup of the backend. Now it can call the **IBadgeHandler**.*calculateNewProgress*(someDataBelongingToGameEvent), which will calculate the new progress. For example, a badge which is acquired when you have correctly responded to 50 items can be implemented this way:

```csharp
class 50ItemsCorrectlyResponded : IBadgeHandler {  
	public string getName() {return “50ItemsCorrectBadge”}  
	public double calculateProgress(object data) {  
		if (item.correct) return oldProgress \+ 1d/50;  
		else return oldProgress;  
	}  
}
```

This new progress calculated by the **IBadgeHandler** will be written back to the corresponding **Badge** record in the database.

**Improvements**

The previous solution has two problems though. The first is that for every new badge that you add, you have to perform two actions. The first is adding a record to the database, second is adding a new class. This is redundant work and is also error-prone. The second problem is that the variables of badges are hard coded within the badge handler classes themselves (for example the amount of items in the previous example). This makes it hard to play with the variables, causes duplicate code and makes the code generally less extendable. An improvement will be described for each of them in the next two sections.

**Problem 1**  
Therefore an improvement can be made. We want as a goal that the classes written in the code simply represent the badges in the database. We can do this the following way.

We add a new interface **IBadgeCompile**, defined the following way

```csharp
interface IBadgeCompile {  
	public Badge compile();  
}
```

Now we merge the two interfaces **IBadgeCompile** and **IBadgeHandler** into a single abstract class. The beautiful thing is that the *compile* method is now merged with the *getName* method. Therefore we can implement **BadgeImage** the following way.

```csharp
abstract class BadgeImage : IBadgeHandler, IBadgeCompile {  
	public Badge compile() {  
		return new Badge {  
			Name \= this.getName()  
			OtherColumns \= … *// could be implemented in virtual methods*  
		}  
	}  
	…  
}
```

This way, the *compile* method used by some compiler class and the *getName* method referred by the **BadgeGameEventHandler**, both refer to the same badge by primary key and therefore we don’t need to worry about whether the database is correctly synced with the code.

Now instead of the badge handlers of the previous section implementing **IBadgeHandler**, they extend **BadgeImage**.   
We add a new class **BadgeImageCollector** with a method *CollectAllBadgeImages*(), which will generate the **BadgeImage** instances. It will scan for all the classes which extend **BadgeImage** and return it in a hashmap (key: string (is *Name*), value: **BadgeImage** (the corresponding **BadgeImage** instance)).  
Now we can create a separate compile script, which will run **BadgeImageCollector**.*CollectAllBadgeImages*() and will use the **BadgeImage**.*compile*() method to populate the database. The **BadgeGameEventHandler** will now also use **BadgeImageCollector**.*CollectAllBadgeImages*() in order to keep track of all the **BadgeImage** instances and use them as described before. As both the compiler and the event handler use the same endpoint, it is ensured that the **IBadgeHandler** implementations at (backend) runtime correspond to the **Badge** records in the database.

**Problem 2**  
The second problem can be improved by parameterizing the **BadgeImage** classes. Simple illustration:

```csharp
class 50ItemsCorrectlyResponded : IBadgeHandler {  
	public string getName() {return “50ItemsCorrectBadge”}  
	public double calculateProgress(object data) {  
		if (item.correct) return oldProgress \+ 1d/50;  
		else return oldProgress;  
	}  
}
```

becomes

```csharp
class ItemsCorrectlyResponded : IBadgeHandler {  
	public int itemAmount;

	public string getName() {return itemAmount.ToString() + “ItemsCorrectBadge”}  
	public double calculateProgress(object data) {  
		if (item.correct) return oldProgress \+ 1d/itemAmount;  
		else return oldProgress;  
	}  
}
```

Now we can create different parameterized versions of **ItemsCorrectlyResponded** in a json file like this:  
{  
“ItemsCorrectlyResponded”: \[  
	variables: {“itemAmount”: 10},  “metaData”: {...}},  
	variables: {“itemAmount”: 50},  “metaData”: {...}},  
	variables: {“itemAmount”: 100},  “metaData”: {...}},  
…  
\],  
“otherBadge”: \[  
	variables: {“var1”: \-32, var2: “sth”, moreVars: …},  “metaData”: {...}},  
	variables: {“var1”: 233, var2: “sthelse”, moreVars: …},  “metaData”: {...}},  
…  
\],  
“evenAnotherBadge”: \[  
	{...variant1},  
	{...variant2},  
…  
\],  
…  
}

The **BadgeImageCollector**.*CollectAllBadgeImages*() method will still scan for all the classes which extend **BadgeImage**, but now it will not simply create an instance of it. It will first scan all the entries in the json (or other kind of data structure). Now it will create a **BadgeImage** instance **for each entry**. It will inject the variables in the json entries into the **BadgeImage** instance, which are then used to change the behavior for each badge based on the variables. Util utility function can be written in the **BadgeImage** class to facilitate or generalise this process. If there are no json entries, it will simply create one instance without any variables.