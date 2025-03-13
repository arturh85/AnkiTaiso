# Method Call Order

Here is the order in which the various methods you can override are called: 

### On Create Order

1. Constructor
1. OnSceneInstantiated
1. Initialise (private)
1. OnParented
1. OnEnterTree
1. _EnterTree
1. _Ready
1. Initialize (public)
1. Setup
1. OnResolved
1. OnBeforeReady
1. OnReady
1. OnAfterReady

Please notice that `_Ready` is called before the dependencies are resolved. 
That happens in `OnResolved` or later `OnReady`.

### On Free Order

1. OnPredelete
1. _ExitTree
1. OnExitTree
1. OnUnparented
1. Dispose
