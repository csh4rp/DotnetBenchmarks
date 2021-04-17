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

Due to this necessary overhead that `async` methods introduce the question arises: <br/>
Should I await or simply return the `Task`? The answer is not so simple,
because apart from the performance there is also slight difference in error handling
(Basically methods which simply return `Task` and do not await can throw and exception before
the task is awaited, `async` methods on the other hand have whole `State Machine` wrapped 
in the `try/catch` block and simply use `SetException` on the `Task` to indicate exception).

Now back to performance, consider two methods:

```c#
private static async Task GetAwaitedTask()
{
     await Task.CompletedTask;
}

private static Task GetTask()
{
    return Task.CompletedTask;
}
```
Each method was invoked 100 000 000 times in loop using code like this:

```c#
for (var i = 0; i < 100_000_000; i++)
{
    await GetTask();
}

for (var i = 0; i < 100_000_000; i++)
{
    await GetAwaitedTask();
}
```

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

Structs bring several advantages over classes 