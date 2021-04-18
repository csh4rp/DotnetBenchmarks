The purpose of this repository is to show the difference in performance between the approaches 
that we can write code in C#. Examples try to achieve the same end result using different
approaches. Keep in mind that the results may vary between the systems and runtimes.
All benchmarks were run on:
* Windows 10.0.19042
* Intel Core i7-7700 CPU 3.60GHz
* .NET Core SDK 5.0.202
* BenchmarkDotNet v0.12.1

**1. Async (AsyncBenchmark)**

In .NET there is some difference between methods that are marked as `async`
and methods which simply return `Task`. Whenever compiler "sees" `async` keyword along
with awaited method it will generate class (or struct) implementing `IAsyncStateMachine` to
handle asynchronous code flow. With "some simplification" this `State Machine` invokes the method
which is awaited and then schedules continuation (the rest of the `async` method - after
the `await` keyword). This continuation can be scheduled run on the same thread as the code 
before the `await` or on the different one (depends on SynchronizationContext or lack of).
Due to this necessary overhead that `async` methods introduce, the question arises:
Should I `await` or simply return the `Task`? The answer is not so simple,
because apart from the performance there is also slight difference in error handling
(basically methods which simply return `Task` and do not `await` can throw the exception before
the `Task` is awaited, `async` methods on the other hand have whole `State Machine` wrapped 
in the `try/catch` block and simply use `SetException` on the `Task` to indicate exception).

The results:

|          Method |     Mean |    Error |   StdDev | Ratio |
|---------------- |---------:|---------:|---------:|------:|
| 'Without await' | 11.01 ns | 0.010 ns | 0.008 ns |  1.00 |
|    'With await' | 23.19 ns | 0.053 ns | 0.047 ns |  2.10 |


**2. False sharing (FalseSharingBenchmark)**

Modern CPUs use couple levels of caching to improve performance, each CPU core can have
separate cache as well as shared cache. When program executes, CPU may loads  
data into the caches, that speeds up the program execution,
but introduces new problem - false sharing. Whenever variable which was loaded into lower
level caches is used by multiple threads, it might happen that some CPU cores will store
the same variable in their caches. Whenever this variable value will change the CPU will need
to reload whole cache line that stores this variable. If variable value changes frequently
it will cause CPU to reload cache lines more frequently which may impact performance.

Consider this array:

```c#
private static readonly int[] Items = new int[ArraySize];
```
This array is used in two methods, both of them create a couple of threads and change the values
in the array. 

Fist method looks like this:

```c#
for (var k = i; k < ArraySize; k += NumberOfTasks)
{
    Items[k] = k;
}
```

The second one looks like this:

```c#
for (var k = startIndex; k < endIndex; k++)
{
    Items[k] = k;
}
```
You should notice that the first method iterates through array by the number of tasks,
so for the first task, with the total of 4 threads it goes by elements like this:
```c#
Items[0], Items[3], Items[7], ...
```

The second method is slightly different, it uses the `startIndex`and `endIndex` variables
for the start and end of the loop and goes though elements between this two numbers sequentially.
This variables are calculated based on the number of threads used and the size of the array,
they basically split array into parts and allow each thread to work on each slice
(`startIndex` and `endIndex` calculation is skipped there for simplicity - see code for the details).
Assuming that we use 4 threads in total and we look at first thread the iteration will look like this:

```c#
Items[0], Items[1], Items[2], ...
```

Using the second method we do small simple trick - we do not operate on the same cache line by
multiple threads (well mostly, because of additional object header which also takes some memory).
This means we do not invalidate cache lines for other threads and they can operate on the data
faster.

Here are the results:

|                           Method |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD |
|--------------------------------- |---------:|---------:|---------:|---------:|------:|--------:|
| 'Run using different cache line' | 164.8 ms |  8.31 ms | 23.83 ms | 158.0 ms |  1.00 |    0.00 |
|  'Run using the same cache line' | 908.3 ms | 11.33 ms | 10.60 ms | 910.2 ms |  4.31 |    0.14 |

**3. Cache line (CacheLineBenchmark)**

This benchmark uses similar principles as described in `False sharing`, but does not include
multiple threads. Example code operates on the multi-dimensional array and retrieves data from it.

The first method accesses elements in the array by "columns", so selects "row" and then iterates "column" by "column",
then it moves to the next "row".

The method looks like this:

```c#
for (var i = 0; i < ArraySize; i++)
{
    for (var j = 0; j < ArraySize; j++)
    {
        _value = Items[i, j];
    }
}
```

The second method iterates through the array using "rows", so with each iteration it goes to the
next row until it reaches the last one, then it goes to the next column and repeats the previous step.

```c#
for (var i = 0; i < ArraySize; i++)
{
    for (var j = 0; j < ArraySize; j++)
    {
        _value = Items[j, i];
    }
}
```

Because of how memory is allocated for such an array, iterating through each element row by row
results in a lot of switches between the cache lines (because each row is in different cache line).
In a big array that might not fit into the lower level cache it might cause a lot of cache reloads
or accessing higher level caches (or RAM), while accessing elements sequentially allows
CPU to read all elements from selected cache line, which is then not used anymore, this results in
much better access time.

Here are the results of the benchmark:

|       Method |     Mean |     Error |    StdDev | Ratio | RatioSD |
|------------- |---------:|----------:|----------:|------:|--------:|
| 'By columns' | 1.174 ms | 0.0010 ms | 0.0009 ms |  1.00 |    0.00 |
|    'By rows' | 2.201 ms | 0.0410 ms | 0.0403 ms |  1.87 |    0.04 |

**4. Structs (StructsBenchmark)**

Structs bring several advantages over classes especially for high performance computing.
Firstly because structs can be created on stack as other value types which do not require
collection by GC. Structs also offer better data locality, all the struct data is kept along
with it, where for the objects you only get reference to the object instance and all it's data
is kept in different part of the memory (keep also in mind the fact that on 64 bit platform
objects can't take less space than 24 bytes and of course additional references to them,
so especially for smaller types struct can use significantly less memory than classes).
Due to the great data locality structs are great candidates for big collection members,
they do not introduce references the same way as collections of classes do. This means
that they have smaller memory footprint and require less cache lines to store which also
translates into better performance, because data can be accessed faster. Of course structs also
have some drawbacks from the performance point of view, first of all they are copied by
value, this means that when big struct is passed to the method as an argument all
of it's field values need to be copied. For big structs that can be some disadvantage, because
a lot of data will need to be copied. Thankfully we can also pass structs as method arguments
by references which eliminates most of the drawbacks. Another part of structs that needs to be
kept in mind is what happens when structs is cast to an interface (either explicit or just passed
as method argument which accepts types implementing specific interface), after such cast struct
is boxed, so most of the advantages are lost at this point, keep that in mind when your structs
implement some interfaces and are passed around. The solution to this problem is quite simple,
just create a method which accepts generic parameter and add a interface type constraint,
this way contract is met and struct will not be boxed. 

Below are shown results of a benchmark which uses different methods to pass argument to a method:

|                   Method |       Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------- |-----------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
|                    Class |  1.2258 ns | 0.0053 ns | 0.0044 ns |  1.00 |    0.00 |      - |     - |     - |         - |
|      'Class - interface' |  3.0091 ns | 0.0055 ns | 0.0052 ns |  2.45 |    0.01 |      - |     - |     - |         - |
|                   Struct |  1.2894 ns | 0.0043 ns | 0.0041 ns |  1.05 |    0.00 |      - |     - |     - |         - |
|            'Struct - in' |  0.9671 ns | 0.0041 ns | 0.0036 ns |  0.79 |    0.00 |      - |     - |     - |         - |
|     'Struct - interface' | 10.3229 ns | 0.0446 ns | 0.0395 ns |  8.42 |    0.03 | 0.0096 |     - |     - |      40 B |
|             'Big struct' | 10.3122 ns | 0.0880 ns | 0.0823 ns |  8.42 |    0.07 |      - |     - |     - |         - |
|        'Big struct - in' |  0.9591 ns | 0.0092 ns | 0.0082 ns |  0.78 |    0.01 |      - |     - |     - |         - |
| 'Big struct - interface' | 29.7187 ns | 0.1159 ns | 0.1028 ns | 24.24 |    0.11 | 0.0306 |     - |     - |     128 B |

**5. Collections (CollectionsBenchmark)**

.NET offers a lot of collections for different purposes, `Stack`, `Queue`, `List`, `HashSet` etc.
While the `Stack` and `Queue` have their unique purposes often `List` and `HashSet` are used for
similar purpose - to keep unique list of items. While most developers know that `HashSet` has
`O(1)` search time complexity and `List` has `O(N)` it is often ignored, because list is simply
more universal. The comparison between this two types is of course not so simple and each scenario
is different, this is why this benchmark focuses mostly on search times (along with collection creation)
based on different sizes and items to that need to be find in these collections.

Here are the benchmark results:

|               Method | itemsToFind |                items |             Mean |         Error |        StdDev |     Gen 0 |  Gen 1 | Gen 2 | Allocated |
|--------------------- |------------ |--------------------- |-----------------:|--------------:|--------------:|----------:|-------:|------:|----------:|
|  'Set - pre created' |  Int32[500] |        HashSet[1000] |      2,265.09 ns |      3.039 ns |      2.694 ns |         - |      - |     - |         - |
|  'Set - pre created' |   Int32[50] |         HashSet[100] |        228.38 ns |      0.427 ns |      0.399 ns |         - |      - |     - |         - |
|  'Set - pre created' |    Int32[5] |          HashSet[10] |         22.41 ns |      0.022 ns |      0.021 ns |         - |      - |     - |         - |
| 'List - pre created' |  Int32[500] |           List[1000] |     43,244.78 ns |     53.418 ns |     47.354 ns |         - |      - |     - |         - |
| 'List - pre created' |   Int32[50] |            List[100] |        537.98 ns |      1.821 ns |      1.703 ns |         - |      - |     - |         - |
| 'List - pre created' |    Int32[5] |             List[10] |         27.05 ns |      0.180 ns |      0.168 ns |         - |      - |     - |         - |
|  'Set - not created' |  Int32[500] |        HashSet[1000] |     75,340.81 ns |    145.213 ns |    121.259 ns |   16.8457 | 1.8311 |     - |   70928 B |
|  'Set - not created' |   Int32[50] |         HashSet[100] |      5,581.66 ns |     13.545 ns |     12.670 ns |    1.7776 |      - |     - |    7464 B |
|  'Set - not created' |    Int32[5] |          HashSet[10] |        441.26 ns |      0.445 ns |      0.416 ns |    0.2503 |      - |     - |    1048 B |
| 'List - not created' |  Int32[500] |           List[1000] |    102,235.63 ns |    221.606 ns |    207.290 ns |    3.7842 |      - |     - |   16248 B |
| 'List - not created' |   Int32[50] |            List[100] |      4,233.90 ns |     13.630 ns |     12.750 ns |    0.4349 |      - |     - |    1848 B |
| 'List - not created' |    Int32[5] |             List[10] |        229.88 ns |      0.656 ns |      0.581 ns |    0.0975 |      - |     - |     408 B |
|           Enumerable |  Int32[500] |     Enumerable[1000] | 28,372,632.50 ns | 47,561.899 ns | 44,489.430 ns | 1437.5000 |      - |     - | 6128000 B |
|           Enumerable |   Int32[50] |      Enumerable[100] |    192,501.68 ns |  3,290.399 ns |  2,568.927 ns |   17.3340 |      - |     - |   72800 B |
|           Enumerable |    Int32[5] |       Enumerable[10] |      1,085.82 ns |      2.340 ns |      2.189 ns |    0.4482 |      - |     - |    1880 B |


**6. Loops (LoopsBenchmark)**

One of the most powerful looping mechanisms in .NET is `foreach` loop, it allows convenient
access to each item of the collection, but when it comes to the high performance code is it
faster than standard `for` loop? The `foreach` "primitives" used under the hood by the `List`
are by themselves pretty high performant. Enumerator used by the `foreach` loop which is used
by the `List` is actually a `struct` which means - no allocations and no garbage collection,
but if you dig a bit deeper you'll see that each `MoveNext` call has one additional check which
compares list version with the version of the list from the moment in time where `Enumerator`
was created. But what if instead of `List`some interface is used? Then another version of
`IEnumerator` is created, this time with allocations - keep that in mind when you iterate over
some collection multiple times.

Here are the benchmarks that compare speed of iteration of the loops:

|                       Method |                items |          Mean |       Error |      StdDev |     Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|----------------------------- |--------------------- |--------------:|------------:|------------:|----------:|--------:|-------:|------:|------:|----------:|
|                   'For loop' |             List[10] |      5.086 ns |   0.0135 ns |   0.0120 ns |      1.00 |    0.00 |      - |     - |     - |         - |
|                   'For loop' |            List[100] |     54.164 ns |   0.0542 ns |   0.0507 ns |     10.65 |    0.03 |      - |     - |     - |         - |
|                   'For loop' |           List[1000] |    509.834 ns |   1.4432 ns |   1.3500 ns |    100.25 |    0.31 |      - |     - |     - |         - |
|                   'For loop' |          List[10000] |  6,288.739 ns |  18.6697 ns |  16.5502 ns |  1,236.40 |    3.86 |      - |     - |     - |         - |
|        'List - Foreach loop' |             List[10] |     34.865 ns |   0.0620 ns |   0.0580 ns |      6.85 |    0.01 |      - |     - |     - |         - |
|        'List - Foreach loop' |            List[100] |    324.598 ns |   0.2838 ns |   0.2654 ns |     63.82 |    0.15 |      - |     - |     - |         - |
|        'List - Foreach loop' |           List[1000] |  3,215.243 ns |  21.6222 ns |  19.1675 ns |    632.14 |    3.80 |      - |     - |     - |         - |
|        'List - Foreach loop' |          List[10000] | 36,387.684 ns |  55.6419 ns |  52.0474 ns |  7,152.41 |   17.57 |      - |     - |     - |         - |
| 'List - foreach lambda loop' |             List[10] |     19.661 ns |   0.0379 ns |   0.0355 ns |      3.87 |    0.01 |      - |     - |     - |         - |
| 'List - foreach lambda loop' |            List[100] |    197.194 ns |   0.2338 ns |   0.2187 ns |     38.77 |    0.10 |      - |     - |     - |         - |
| 'List - foreach lambda loop' |           List[1000] |  1,720.699 ns |   2.8386 ns |   2.5164 ns |    338.30 |    0.77 |      - |     - |     - |         - |
| 'List - foreach lambda loop' |          List[10000] | 19,556.221 ns | 362.6541 ns | 321.4834 ns |  3,844.93 |   67.22 |      - |     - |     - |         - |
| 'IEnumerable - foreach loop' |             List[10] |     97.512 ns |   0.2026 ns |   0.1796 ns |     19.17 |    0.06 | 0.0095 |     - |     - |      40 B |
| 'IEnumerable - foreach loop' |            List[100] |    748.238 ns |   0.7204 ns |   0.6738 ns |    147.12 |    0.35 | 0.0095 |     - |     - |      40 B |
| 'IEnumerable - foreach loop' |           List[1000] |  7,969.585 ns |  22.3532 ns |  19.8155 ns |  1,566.87 |    5.31 |      - |     - |     - |      40 B |
| 'IEnumerable - foreach loop' |          List[10000] | 79,269.983 ns |  52.0024 ns |  43.4244 ns | 15,584.99 |   38.96 |      - |     - |     - |      40 B |

**7. Enums (EnumsBenchmark)**

Enums are very useful features, that can bring some meaning into what would normally be a number
(at least from programmers point of view). Often they are just used to simply compare some values,
but sometimes they have custom attributes attached to the members and their names are mapped to
string (via `ToString`). This might not sound like a big of an issue until you realize that
each `ToString` call allocates new `String` and is not as fast, not to mention usage of `Reflection`
to retrieve some attribute information. For cases where such things happen in "hot paths"
`Enumerations` are great solution. They allow you to easily add some additional properties and
methods to what else would be simple enum.

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

In order to retrieve value of `Description` attribute reflection needs to be used which can be
quite slow in "hot paths". To solve this issue this value should be cached or `Enumeration`
like in code below can be used:

```c#
public sealed class OptionsEnumeration : Enumeration<OptionsEnumeration>
{
    private OptionsEnumeration(int id, string name, string description)
     : base(id, name, description)
    {
    }

    public static readonly OptionsEnumeration Read = new(0, "Read", "Read");
    public static readonly OptionsEnumeration Write = new(1, "Write", "Write");
    public static readonly OptionsEnumeration Delete = new(2, "Delete", "Delete");
}
```
All available instances of this type are cached and immutable which allows for easy reuse.

Here are benchmark results of different actions performed by the `Enums`and `Enumeratons`:

|                      Method |         Mean |     Error |    StdDev |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------------------- |-------------:|----------:|----------:|-------:|------:|------:|----------:|
|           'Enum - ToString' |    23.757 ns | 0.0823 ns | 0.0770 ns | 0.0057 |     - |     - |      24 B |
|    'Enumeration - ToString' |     2.002 ns | 0.0026 ns | 0.0025 ns |      - |     - |     - |         - |
|        'Enum - Description' | 1,213.760 ns | 6.3189 ns | 5.9107 ns | 0.0687 |     - |     - |     288 B |
| 'Enumeration - Description' |     1.685 ns | 0.0031 ns | 0.0029 ns |      - |     - |     - |         - |
|              'Enum - Parse' |    49.767 ns | 0.0698 ns | 0.0653 ns |      - |     - |     - |         - |
|       'Enumeration - Parse' |    11.142 ns | 0.0516 ns | 0.0458 ns |      - |     - |     - |         - |

**8. Virtual Dispatch (VirtualDispatchBenchmark)**

Programming languages such as `C#` which support polymorphism need a away to know which method
needs to be called based on the type. For types that simply inherit from another type standard
`VTables`are used to see which member needs to be called. For `interfaces` this is slightly
different, mechanism called `Virtual Stub Dispatch` is used. It works in similar way as the `VTable`,
but might require few more steps to get the right method to call. To find the right method
runtime needs to check current type `Slot map` (which is a table that holds information about
methods for which current type implements body, it holds references to current type methods
implementation table index). If current type implements specified `interface`, `Slot map` will
contain entry for that interface with the `VTable` index for the method. For types that simply
inherit `interface` implementation from the base class there will be no no slot map entry for
the interface type, search will be then continued in the base class, is `Slot Map` entry for
interface will be found, based on the`Scope` value of the entry the next value in the entry
should be interpreted as `Virtual Slot` index for current type or as `VTable` index.
Another optimization that can be used to speed up `Virtual Method` calls can be usage of `sealed`
keyword. From the assembly point of view type or method marked with this keyword can be inlined
which can bring some performance improvements in code "hot paths".

Here are benchmark results of different approaches to invoke type methods
(for 1000 method calls - because methods are quite small):

|                 Method |       Mean |   Error |  StdDev | Ratio | RatioSD |
|----------------------- |-----------:|--------:|--------:|------:|--------:|
|          'Static call' |   247.5 ns | 0.52 ns | 0.48 ns |  1.00 |    0.00 |
|          'Direct call' |   488.3 ns | 1.24 ns | 1.16 ns |  1.97 |    0.01 |
| 'Sealed Subclass call' |   486.4 ns | 0.46 ns | 0.41 ns |  1.96 |    0.00 |
|        'SubClass call' | 1,447.7 ns | 2.30 ns | 2.15 ns |  5.85 |    0.02 |
|       'BaseClass call' | 1,446.4 ns | 0.73 ns | 0.61 ns |  5.84 |    0.01 |
|       'Interface call' | 1,929.6 ns | 3.80 ns | 3.37 ns |  7.79 |    0.02 |

**9. Dynamic Method Invocation (IndirectCallBenchmark)**

In .NET in order to call method on type which is dynamically resolved on runtime usually reflection
is used, but it's actually one of few mechanisms available to do so (and the slowest).
Other mechanisms include usage of `dynamic` keyword which with help of `Call Site Caching`
which basically caches such method calls allows for much faster method invocation. There is also
third mechanism which is rarely used - dynamic method generation. Using `System.Reflection.Emit`
namespace and some knowledge of `CIL` we are able to create delegate for method which in certain
scenarios can be even faster than dynamic (take look at code for the details).

Here are benchmark results (method called 1000 times to better use `Call Site Caching` for `dynamic`):

|                  Method |         Mean |     Error |    StdDev |  Ratio | RatioSD |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------------------ |-------------:|----------:|----------:|-------:|--------:|--------:|------:|------:|----------:|
| 'Direct interface call' |     246.6 ns |   0.28 ns |   0.24 ns |   1.00 |    0.00 |       - |     - |     - |         - |
| 'Emitted delegate call' |   2,167.4 ns |   1.51 ns |   1.26 ns |   8.79 |    0.01 |       - |     - |     - |         - |
|          'Dynamic call' |   7,103.9 ns |  32.30 ns |  30.21 ns |  28.80 |    0.14 |  5.7373 |     - |     - |   24000 B |
|       'Reflection call' | 160,274.6 ns | 426.04 ns | 377.67 ns | 650.04 |    1.97 | 15.1367 |     - |     - |   64088 B |

**10. String concatenation (StringsBenchmark)**

Most developers know that when it comes to dynamic string building `StringBuilder`is the best
approach, it (usually) uses less memory is faster than other string concatenation methods,
but when it comes to building simple string from just a few parameters (like DB key) what
is better: `String.Concat` or `String.Format`? You might not even realize that code like this:

```c#
var str = $"String building: {parameter}";
```

will be translated into something like this:

```c#
var str = string.Format("String building: {0}", parameter);
```

and a call like this:

```c#
var str = "String building: " + parameter;
```

into:

```c#
var str = string.Concat("String building: ", parameter);
```

Under the hood both of these methods use different approaches to building a final string.
If you don't really need any custom formatting, then `string.Concat` will usually be faster
than `string.Format`, but it will use slightly more memory (`string.Format` allocates some
of memory it uses on the `Stack`), so you need to decide for yourself what's more important.

Here are the results:

|        Method | parameters |          format |      Mean |    Error |   StdDev | Gen 0  | Gen 1 | Gen 2 | Allocated |
|-------------- |----------- |---------------- |----------:|---------:|---------:|-------:|------:|------:|----------:|
| String.Concat |  Object[2] |               ? |  21.73 ns | 0.124 ns | 0.116 ns | 0.0210 |     - |     - |      88 B |
| String.Concat |  Object[3] |               ? |  28.70 ns | 0.159 ns | 0.141 ns | 0.0249 |     - |     - |     104 B |
| String.Concat |  Object[4] |               ? |  36.58 ns | 0.143 ns | 0.119 ns | 0.0287 |     - |     - |     120 B |
| String.Concat |  Object[5] |               ? |  40.66 ns | 0.134 ns | 0.118 ns | 0.0344 |     - |     - |     144 B |
|               |            |                 |           |          |          |        |       |       |           |
| String.Format |  Object[2] |          {0}{1} |  70.70 ns | 0.223 ns | 0.198 ns | 0.0114 |     - |     - |      48 B |
| String.Format |  Object[3] |       {0}{1}{2} |  88.10 ns | 0.193 ns | 0.171 ns | 0.0134 |     - |     - |      56 B |
| String.Format |  Object[4] |    {0}{1}{2}{3} | 115.10 ns | 0.441 ns | 0.368 ns | 0.0153 |     - |     - |      64 B |
| String.Format |  Object[5] | {0}{1}{2}{3}{4} | 129.27 ns | 0.228 ns | 0.213 ns | 0.0191 |     - |     - |      80 B |

**11. LINQ (LinqBenchmark)**

LINQ is very convenient way of operation on collections, is allows to write easy to read
code that mostly replaces old loops (but uses them under the hood), but what is the cost of
that convenience? Depending on the collection used, LINQ might allocate some memory for the
iterators it uses to achieve the result, it also does some additional checks.

Here are the benchmark results:

|         Method |                items | minId | maxId |        Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------------- |--------------------- |------ |------ |------------:|---------:|---------:|------:|--------:|-------:|------:|------:|----------:|
|     'For loop' |           List [100] |    50 |    60 |    188.0 ns |  1.38 ns |  1.23 ns |  1.00 |    0.00 | 0.0782 |     - |     - |     328 B |
| 'Foreach loop' |           List [100] |    50 |    60 |    493.5 ns |  0.54 ns |  0.51 ns |  2.63 |    0.02 | 0.0782 |     - |     - |     328 B |
|           Linq |           List [100] |    50 |    60 |    463.5 ns |  1.39 ns |  1.30 ns |  2.47 |    0.02 | 0.1354 |     - |     - |     568 B |
|                |                      |       |       |             |          |          |       |         |        |       |       |           |
|     'For loop' |          List [1000] |   500 |   600 |  1,560.1 ns |  2.75 ns |  2.44 ns |  1.00 |    0.00 | 0.5226 |     - |     - |    2192 B |
| 'Foreach loop' |          List [1000] |   500 |   600 |  4,296.8 ns | 10.99 ns |  9.74 ns |  2.75 |    0.01 | 0.5188 |     - |     - |    2192 B |
|           Linq |          List [1000] |   500 |   600 |  3,266.3 ns |  4.38 ns |  3.88 ns |  2.09 |    0.00 | 0.5798 |     - |     - |    2432 B |
|                |                      |       |       |             |          |          |       |         |        |       |       |           |
|     'For loop' |         List [10000] |  5000 |  6000 | 19,327.2 ns | 21.30 ns | 16.63 ns |  1.00 |    0.00 | 3.9673 |     - |     - |   16600 B |
| 'Foreach loop' |         List [10000] |  5000 |  6000 | 45,121.9 ns | 67.19 ns | 59.56 ns |  2.34 |    0.00 | 3.9673 |     - |     - |   16600 B |
|           Linq |         List [10000] |  5000 |  6000 | 33,800.8 ns | 40.69 ns | 38.06 ns |  1.75 |    0.00 | 3.9673 |     - |     - |   16840 B |