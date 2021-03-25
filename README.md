# Object Graph
The object graph is a dependency graph framework for .net. 

## Summary
The framework is a classic dependency graph with a few additional features:
 * a child graph can be created which only builds what has explicitly changed / whose dependencies have changed
 * lockable dependencies, i.e. the graph gets broken, in child graphs. This can either be locking an object or locking a dependency of one object from another, whilst leaving the object free to be updated for use by other dependencies (see diagram for explanation later)
 * building is in 2 steps:
   * dependency reporting
   * object building 

### Key components / concepts
* object context
  * the actual graph object
  * defined as an optional parent context and an object builder provider (can provide fixed values for leaf nodes)
* object builders - the components which do the actual building; 2 features:
  * reporting dependencies
  * building objects for a given set of dependencies
* task runner - a caller can choose how the actual tasks are run (for thread limiting etc.)

## Recommend usages
This component was originally put together to allow the construction of a perturbable calculation graph representing financial markets, e.g. a small snippet might be

        A
       / \
      /   \
     B     C
          / \
         /   \
        D     E
        
Where some of the objects may be consumed directly in the output process and / or be consumed as intermediate objects. 

For the purposes of calculating bump risk, it might be necessary to do a transformation of E -> E', this would entail the rebuilding of C and subsequently of A whilst objects D and B would not be updated.

        A                                                A'
       / \                                              / \
      /   \                                            /   \
     B     C           + E'             ->            B     C'
          / \                                              / \
         /   \                                            /   \
        D     E                                          D     E'

** Note that the object building in the child object context is only triggered when that object context is asked for the address. This means that one can have a very populated base context and create a child context with a perturbation and only rebuild those objects which are actually needed **

## FAQs

### Can an object's dependencies depend upon the value of other dependencies?
Yes - look at GetAdditionalDependencies. 

### How do I configure my graph?
Because there are multiple different possible use-cases here, there is no explicit 'one true way' to do this. In general, what we've found works very well is having:

 * use an address which is composed of 'object type' - 'type specific parameters' 
 * an object builder provider which provides builders based off 'object type' as well as optionally providing the ability to override for specific addresses
 
### How do I lock a dependency between A and B
When the object builder for object A reports its dependency on B, it can specify the object context from which B must come from. If left null, then the dependency will logically come from the current object context etc.

In this mode, other objects which depend on B will get rebuilt as necessary

### What happens if a dependency fails to build
Other objects will build as expected, the offending object will be reported as a failure and the exception can be obtains.

### How do I lock a specific object
In this case, extract the desired object from the context which has the value and apply it as a scenario. Note that this can trigger a rebuild of dependent objects.

### How multi-threaded is this?
The graph itself is in 2 parts:
1. The concept of a graph and dependencies
2. The building of nodes

The graph works in an asynchronous fashion with all interactions with it for addresses returning objects containing wait handles, so a user can request multiple values to be constructed and come back to them later. Dependencies by default are generated at the same time as the request for them to be built is originally mdae as they're assumed to be quick to be generated, but an object builder may flag to the framework that they'll take a while at which point, they'll be run on the task queue.

Node building is delegated to a 'task runner' which is passed in. This is given tasks as soon as they are logically ready to be run and so the choice of how parallelisation is performned is entirely up to the task runner. The usual approach here is to have a multithreaded runner, with 1 thread per available core to maximise throughput. However, the task runners may do something more clever based off requirements, e.g. in one use-case builders might be split into 2 use-cases, data fetching and CPU bound items; the task runner could maintain 2 job queues / threadpools to maximise performance/

If however, one wishes to assign only a certain portion of a system's resources to this context or even run in a single task at a time mode, then that can be done by providing an appropriate configured task runner.

### I have a new feature / performance improvement / suggestion etc.
Please either raise an issue here or contact us.
