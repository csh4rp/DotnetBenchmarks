The purpose of this repository is to show the difference in performance between the ways that we can write code in C#. 
Examples try to achieve the same end result using different approaches.
Keep in mind that the results may vary between the systems and runtimes
(this is why below each benchmark basic system and runtime information is shown).

1. Async (AsyncBenchmark)

In .NET there is some difference between methods that are marked as `async`
and methods which simply return `Task`.
Whenever compiler "sees" `async` keyword along with `awaited` method
it will generate class (or struct) implementing `IAsyncStateMachine` to handle asynchronous code flow. 
With some oversimplification this `State Machine` invokes the method which is awaited
and then schedules continuation (the rest of the async method - after the await keyword).
This continuation can be run on the same thread as the code before the `await` or on the different one
(depends on SynchronizationContext or lack of).
Due to this necessary overhead that `async` methods introduce the question arises:
Should I await or simply return the `Task`? The answer is not so simple,
because apart from the performance there is also slight difference in error handling
(Basically methods which simply return `Task` and do not await can throw and exception before
the task is awaited, `async` methods on the other hand have whole `State Machine` wrapped 
in the `try/catch` block and simply use `SetException` on the `Task` to indicate exception).

Here are benchmark results 

The results:


2. False sharing (FalseSharingBenchmark)

Modern CPUs use couple levels of caching to improve performance,
each CPU core can have separate cache as well as shared cache.
When program executes, CPU may load frequently used data into the
lower level caches (not shared), that speeds up the program execution,
but introduces new problem - false sharing. Whenever variable which was
loaded into lower level caches is used by multiple threads, it might happen
that some CPU cores will store the same variable in their caches.
Whenever this variable value will change the CPU will need to reload
whole cache line that stores this variable. If variable value changes
frequently it will cause CPU to reload cache lines more frequently
which might impact performance.

Consider this array:

```c#
private static readonly int[] ItemsArray = new int[ArraySize];
```
This array is used in two methods, both of them create a couple of threads
and change the values in the array. 

Fist method looks like this:

```c#
for (var k = i; k < ArraySize; k += NumberOfTasks)
{
    ItemsArray[k] = k;
}
```

The second one looks like this:

```c#
for (var k = startIndex; k < endIndex; k++)
{
    ItemsArray[k] = k;
}
```
You should notice that the first method iterates through array by the 
number of tasks, so for the first task, with the total of 4 threads
it goes by elements like this:
```c#
ItemsArray[0], ItemsArray[3], ItemsArray[7], ...
```

The second method is slightly different, it uses the `startIndex` 
and `endIndex` variables for the start and end of the loop and
goes though elements between this two numbers sequentially.
This variables are calculated based on the number of threads used
and the size of the array, they basically split array into parts
and allow each thread to work on each slice (`startIndex` and `endIndex`
calculation is skipped there for simplicity - see code for the details).
Assuming that we use 4 threads in total and we look at first thread
the iteration will look like this:

```c#
ItemsArray[0], ItemsArray[1], ItemsArray[2], ...
```

Using the second method we do small simple trick - we do not operate
on the same cache line in multiple threads
(well mostly, because of additional object header which also takes some memory).
which means we do not invalidate cache lines for other threads
and they can operate on the data faster.

Here are the results:


3. Cache line (CacheLineBenchmark)

This benchmark uses similar principles as described in `False sharing`,
but does not include multiple threads. Example code operates on the
multi-dimensional array and retrieves data from it.

The first method looks accesses elements in the array by "columns", so
select array at the specified index of the array and goes through each element
of selected array. The method looks like this:

```c#
var sum = 0L;
for (var i = 0; i < ItemsArray.Length; i++)
{
    for (var j = 0; j < ArraySize; j++)
    {
        sum += ItemsArray[i, j];
    }
}
```

The second method iterates through the array using "rows", so with each iteration
it goes to the next row until it reaches the last one, then it goes to the next column.

```c#
var sum = 0L;
for (var i = 0; i < ArraySize; i++)
{
    for (var j = 0; j < ItemsArray.Length; j++)
    {
        sum += ItemsArray[j, i];
    }
}
```

Because of how memory is allocated for such an array iterating through each element row by row
results in a lot of switches between the cache lines (because each row is in different cache line).
In a big array that might not fit into the lower level cache it might cause a lot of cache reloads
or accessing higher level caches (which are slower), while accessing elements sequentially allows
CPU to read all elements from selected cache line, which is then not used anymore, this results in
much better access time.

Here are the results of the benchmark:


4. Structs (StructsBenchmark)

Structs bring several advantages over classes especially for high performance
computing. Firstly because structs can be created on stack as other value types
which does not require collection by GC. Struct also offers better data locality, 
all the struct data is kept along with it, where for the objects you only get reference
to the object instance and all it's data is kept in different part of the memory
(keep also in mind the fact that on 64 bit platform objects can't take less space than 24 bytes
and of course additional references to them, so especially for smaller types struct can use
significantly less memory than objects).
Due to to great data locality structs are great candidates for collection members,
they do not introduce references the same way as collections of objects do. This means
that they have smaller memory footprint and require less cache lines to store which also
translates into better performance, because data can be accessed faster.<br/>
Of course structs also introduce some drawbacks from the performance point of view,
first of all they are copied by value (by default), this means that when big struct is passed
to the method as an argument all of it's field values need to be copied. For big structs
that can be some disadvantage, because a lot of data will need to be copied. Thankfully
we can also pass structs as method arguments by references which eliminates most of the drawbacks.
Another part of structs that needs to be kept in mind is what happens when structs is
cast to an interface (either explicit or just passed as method argument which accepts types
implementing specific interface), after such cast struct is boxed, so most of the advantages are
lost at this point, keep that in mind when your structs implement some interfaces and are passed around.
The solution to this problem is quite simple, just create a method which accepts generic parameter
and add a interface type constraint, this way contract is met and struct will not be boxed. 

Below are shown results of a benchmark which uses different methods to pass argument to a method:


5. Collections (CollectionsBenchmark)

.NET offers a lot of collections for different purposes, `Stack`, `Queue`, `List`, `HashSet` etc.
While the `Stack` and `Queue` have their unique purposes often `List` and `HashSet` are used for similar
purpose - to keep unique list of items. While most developers know that `HashSet` has `O(1)` search
time complexity and `List` has `O(N)` it is often ignored, because list is simply more universal.
The comparison between this two types is of course not so simple and each scenario is different this
is why this benchmark focuses only on search times (along with collection creation) based on
different sizes and items to that need to be find in these collections.

Here are the benchmark results:


6. Loops (LoopsBenchmark)

One of the most powerful looping mechanisms in .NET is `foreach` loop,
it allows convenient access to each item of the collection, but when it comes
to the high performance code is it faster than standard `for` loop?
The `foreach` "primitives" used under the hood by the `List` are by themselves
pretty high performant. Enumerator used by the `foreach` loop which is used
by the `List` is actually a `struct` which means - no allocations and
no garbage collection, but if you dig a bit deeper you'll see that each `MoveNext` call
has one additional check which compares list version with the version of the list
from the moment in time where `Enumerator` was created. But what if instead of `List`
some interface is used? Then another version of `IEnumerator` is created, this time
with allocations - keep that in mind when you iterate over some collection multiple times.

Here are the benchmarks that compare speed of iteration of the loops:


7. Enums (EnumsBenchmark)

Enums are very useful features, that can bring some meaning into what
would normally be a number (at least from programmers point of view).
Often they are just used to simply compare some values, but sometimes
they have custom attributes attached to the members and their names
are mapped to string (via `ToString`). This might not sound like a big of
an issue until you realize that each `ToString` call allocates new `String`
and is not as fast, not to mention usage of `Reflection` to retrieve some
attribute information. For cases where such things happen in "hot paths"
`Enumerations` are great solution. They allow you to easily add some additional 
properties and methods to what else would be simple enum.

Consider enum like this;

```c#
public enum Options
{
    [Description("Read")]
    Read = 0,
    
    [Description( "Write")]
    Write = 1,
    
    [Description("Delete")]
    Delete = 2
}
```

In order to retrieve value of `Description` attribute reflection needs to be
used which can be quite slow in "hot paths". To solve this issue this value
should be cached or `Enumeration` like in code below can be used:

```c#
public sealed class OptionsEnumeration : Enumeration<OptionsEnumeration>
{
    private OptionsEnumeration(int id, string name, string description) : base(id, name, description)
    {
    }

    public static readonly OptionsEnumeration Read = new(0, "Read", "Read");
    public static readonly OptionsEnumeration Write = new(1, "Write", "Write");
    public static readonly OptionsEnumeration Delete = new(2, "Delete", "Delete");
}
```
All available instances of this type are cached and immutable which allows
for easy reuse.

Here are benchmark results of different actions performed by the `Enums`
and `Enumeratons`:

8. Virtual Dispatch (VirtualDispatchBenchmark)

Programming languages such as `C#` which support polymorphism need
a away to know which method needs to be called based on the type.
For types that simply inherit from another type standard `VTables`
are used to see which member needs to be called. For `interfaces` this is
slightly different, mechanism called `Virtual Stub Dispatch` is used.
It works in similar way as the `VTable`, but might require few more steps
to get the right method to call. To find the right method runtime needs to
check current type `Slot map` (which is a table that holds information about
methods for which current type implements body, it holds references to
current type methods implementation table index).
If current type implements specified `interface`, `Slot map` will contain
entry for that interface with the `VTable` index for the method. For types that simply
inherit `interface` implementation from the base class there will be no no
slot map entry for the interface type, search will be then continued in 
the base class, is `Slot Map` entry for interface will be found, based on the
`Scope` value of the entry the next value in the entry should be interpreted
as `Virtual Slot` index for current type or as `VTable` index.
Another optimization that can be used to speed up `Virtual Method` calls
can be usage of `sealed` keyword. From the assembly point of view type or
method marked with this keyword can be inlined which can bring some performance
improvements in code "hot paths".

Here are benchmark results of different approaches to invoke type methods:

9. Dynamic Method Invocation (IndirectCallBenchmark)

In .NET in order to call method on type which is dynamically resolved on runtime
usually reflection is used, but it's actually one of few mechanisms available to
do so (and the slowest). Other mechanisms include usage of `dynamic` keyword
which with help of `Call Site Caching` which basically caches such method calls
allows for much faster method invocation. There is also third mechanism which is
rarely used - dynamic method generation. Using `System.Reflection.Emmit`
namespace and some knowledge of `CIL` we are able to create delegate for method
which in certain scenarios can be even faster than dynamic (take look at code for
the details).