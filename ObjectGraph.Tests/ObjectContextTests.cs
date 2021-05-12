using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BorsukSoftware.ObjectGraph
{
	[TestClass]
	public class ObjectContextTests
	{
		#region Member variables

		private Tasks.TaskRunner _taskRunner = new Tasks.TaskRunner( null );

		#endregion

		#region Object building (simple)

		[TestMethod]
		public void BuildSimpleObject()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = true;
			var objectBuilderProvider = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( null, fixedObjects );
			var context = new ObjectContext<int>( null, objectBuilderProvider, _taskRunner );

			var info = context.BuildObject( 0 );
			Assert.IsNotNull( info );
			info.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, info.ObjectBuildingState );
			Assert.IsInstanceOfType( info.BuiltObject, typeof( bool ) );
			Assert.IsTrue( (bool) info.BuiltObject );
		}

		[TestMethod]
		[Description( "Checks the builder of an object hierarchy where everything is from the same context")]
		public void BuildSimpleHeirarchySameContext()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = "0";
			fixedObjects [ 1 ] = "1";

			var builders = new Dictionary<int, ObjectBuilders.IObjectBuilder<int>>();
			builders [ 2 ] = new ObjectBuilders.FunctionBasedObjectBuilder<int>(
				( builderContext, address ) =>
				{
					var dependencySet = new ObjectBuilders.DependencySet<int>();
					dependencySet.AddDependency( "0", 0 );
					dependencySet.AddDependency( "1", 1 );
					return dependencySet;
				},
				( builderContext, address, dependencies ) => null,
				( builderContext, address, dependencies ) => { 
					Assert.IsTrue( dependencies.TryGetDependency( "0", out var dependencyNumber1 ) );
					Assert.IsTrue( dependencies.TryGetDependency( "1", out var dependencyNumber2 ) );

					return address.ToString();
				} );

			var obp = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( builders, fixedObjects );
			var context = new ObjectContext<int>( null, obp, _taskRunner );

			var info = context.BuildObject( 2 );
			Assert.IsNotNull( info );
			info.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, info.ObjectBuildingState );
			Assert.IsInstanceOfType( info.BuiltObject, typeof( string ) );
			Assert.AreEqual( "2", (string) info.BuiltObject );
		}

		#endregion

		#region Negative Tests

		[TestMethod]
		public void MissingBuilder()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = "0";
			fixedObjects [ 1 ] = "1";

			var obp = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( null, fixedObjects );
			var context = new ObjectContext<int>( null, obp, _taskRunner );

			var info = context.BuildObject( 2 );
			Assert.IsNotNull( info );
			info.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.NoBuilderAvailable, info.ObjectBuildingState );
		}

		#endregion

		#region Override functionality

		[TestMethod]
		public void SimpleDependencyOverride()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = "0";
			fixedObjects [ 1 ] = "1";

			var builders = new Dictionary<int, ObjectBuilders.IObjectBuilder<int>>();
			builders [ 2 ] = new ObjectBuilders.FunctionBasedObjectBuilder<int>(
				( builderContext, address ) =>
				{
					var dependencySet = new ObjectBuilders.DependencySet<int>();
					dependencySet.AddDependency( "0", 0 );
					dependencySet.AddDependency( "1", 1 );
					return dependencySet;
				},
				( builderContext, address, dependencies ) => null,
				( builderContext, address, dependencies ) => {
					Assert.IsTrue( dependencies.TryGetDependency( "0", out var dependencyNumber1 ) );
					Assert.IsTrue( dependencies.TryGetDependency( "1", out var dependencyNumber2 ) );

					var builtObject = string.Format( "{0}-{1}-{2}", 
						address, 
						dependencyNumber1.BuiltObject, 
						dependencyNumber2.BuiltObject );
					return builtObject;
				} );

			var obp = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( builders, fixedObjects );
			var context = new ObjectContext<int>( null, obp, _taskRunner );

			var info = context.BuildObject( 2 );
			Assert.IsNotNull( info );
			info.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, info.ObjectBuildingState );
			Assert.IsNotNull( info.BuiltObject );
			Assert.IsInstanceOfType( info.BuiltObject, typeof( string ) );
			Assert.AreEqual( "2-0-1", (string) info.BuiltObject );

			var overrides = new Dictionary<int, object>();
			overrides [ 0 ] = "Update";
			var overrideOBP = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( null, overrides );
			var overrideContext = new ObjectContext<int>( context, overrideOBP, _taskRunner );

			var overrideInfo = overrideContext.BuildObject( 2 );
			Assert.IsNotNull( overrideInfo );
			overrideInfo.ObjectBuiltOrFailureWaitHandle.WaitOne();

			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, overrideInfo.ObjectBuildingState );
			Assert.AreNotSame( info.BuiltObject, overrideInfo.BuiltObject );
			Assert.IsInstanceOfType( overrideInfo.BuiltObject, typeof( string ) );
			Assert.AreEqual( "2-Update-1", overrideInfo.BuiltObject );
		}

		[TestMethod]
		public void OverrideWithDependencyChanges()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = "0";
			fixedObjects [ 1 ] = "1";
			fixedObjects [ 2 ] = "2";

			var builders = new Dictionary<int, ObjectBuilders.IObjectBuilder<int>>();
			builders [ 3 ] = new ObjectBuilders.FunctionBasedObjectBuilder<int>(
				( builderContext, address ) =>
				{
					var dependencySet = new ObjectBuilders.DependencySet<int>();
					dependencySet.AddDependency( "0", 0 );
					dependencySet.AddDependency( "1", 1 );
					return dependencySet;
				},
				( builderContext, address, dependencies ) => null,
				( builderContext, address, dependencies ) => {
					Assert.IsTrue( dependencies.TryGetDependency( "0", out var dependencyNumber1 ) );
					Assert.IsTrue( dependencies.TryGetDependency( "1", out var dependencyNumber2 ) );

					var builtObject = string.Format( "{0}-{1}-{2}", 
						address, 
						dependencyNumber1.BuiltObject, 
						dependencyNumber2.BuiltObject );
					return builtObject;
				} );

			var obp = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( builders, fixedObjects );
			var context = new ObjectContext<int>( null, obp, _taskRunner );

			var buildingInfo3 = context.BuildObject( 3 );
			buildingInfo3.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, buildingInfo3.ObjectBuildingState );
			Assert.AreEqual( "3-0-1", (string) buildingInfo3.BuiltObject );

			var buildersOverrides = new Dictionary<int, ObjectBuilders.IObjectBuilder<int>>();
			buildersOverrides  [ 3 ] = new ObjectBuilders.FunctionBasedObjectBuilder<int>(
				( builderContext, address ) =>
				{
					var dependencySet = new ObjectBuilders.DependencySet<int>();
					dependencySet.AddDependency( "1", 1 );
					dependencySet.AddDependency( "2", 2 );
					return dependencySet;
				},
				( builderContext, address, dependencies ) => null,
				( builderContext, address, dependencies ) => {
					Assert.IsTrue( dependencies.TryGetDependency( "1", out var dependencyNumber1 ) );
					Assert.IsTrue( dependencies.TryGetDependency( "2", out var dependencyNumber2 ) );

					var builtObject = string.Format( "{0}-{1}-{2}", 
						address, 
						dependencyNumber1.BuiltObject, 
						dependencyNumber2.BuiltObject );
					return builtObject;
				} );

			var obpOverrides = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( buildersOverrides, null );
			var overrideContext = new ObjectContext<int>( context, obpOverrides, _taskRunner );

			var overridenBuildingInfo3 = overrideContext.BuildObject( 3 );
			overridenBuildingInfo3.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, overridenBuildingInfo3.ObjectBuildingState );
			Assert.AreEqual( "3-1-2", (string) overridenBuildingInfo3.BuiltObject );
		}

		#endregion

		#region Bulk Building

		[TestMethod]
		public void ParallelBuilding()
		{
			var functionBasedOBP = new ObjectBuilders.Providers.FunctionBasedObjectBuilderProvider<Tuple<string, int>>(
				( tuple ) =>
				{
					switch( tuple.Item1 )
					{
						case "Data":
							return new ObjectBuilders.FixedObjectBuilder<Tuple<string, int>>( 0 );

						case "Logic":
							return new ObjectBuilders.FunctionBasedObjectBuilder<Tuple<string, int>>(
								( builderContext, address ) =>
								{
									var dependencySet = new ObjectBuilders.DependencySet<Tuple<string, int>>();
									dependencySet.AddDependency( "dep" + address.Item2, Tuple.Create( "Data", address.Item2 ) );
									return dependencySet;
								},
								( builderContext, address, dependencies ) => null,
								( builderContext, address, dependencies ) =>
								{
									var startTime = DateTime.UtcNow;
									double val = 1;
									for( int i = 0; i < 100000000; ++i )
										val += Math.Sqrt( i );

									var timeSpan = DateTime.UtcNow - startTime;
									return timeSpan;
								} );

						default:
							throw new NotSupportedException();
					}
				} );

			var context = new ObjectContext<Tuple<string, int>>( null, functionBasedOBP, _taskRunner );
			var infoObjects = new List<IObjectBuildingInfo<Tuple<string, int>>>( 300 );
			for( int  idx = 0; idx< 16; idx++ ) {
				var address = Tuple.Create( "Logic", idx );
				infoObjects.Add( context.BuildObject( address ) );
			}

			foreach( var infoObj in infoObjects )
			{
				infoObj.ObjectBuiltOrFailureWaitHandle.WaitOne();
				Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, infoObj.ObjectBuildingState );
			}
		}

		#endregion

		#region ObjectLocking

		[TestMethod]
		public void CheckObjectLocking()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = "0a";

			var fixedObjects2 = new Dictionary<int, object>();
			fixedObjects2 [ 0 ] = "0b";

			var builders = new Dictionary<int, ObjectBuilders.IObjectBuilder<int>>();
			var obp = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( builders, fixedObjects );
			var context = new ObjectContext<int>( null, obp, _taskRunner );

			obp.ObjectBuilders [ 1 ] = new ObjectBuilders.FunctionBasedObjectBuilder<int>(
				( builderContext, address ) =>
				{
					var dependencySet = new ObjectBuilders.DependencySet<int>();
					dependencySet.AddDependency( "live", 0, null );
					dependencySet.AddDependency( "fixed", 0, context );
					return dependencySet;
				},
				( builderContext, address, dependencies ) => null,
				( builderContext, address, dependencies ) => {
					Assert.IsTrue( dependencies.TryGetDependency( "live", out var liveDependency ) );
					Assert.IsTrue( dependencies.TryGetDependency( "fixed", out var fixedDependency ) );

					var builtObject = string.Format( "{0}-{1}-{2}", 
						address, 
						fixedDependency.BuiltObject, 
						liveDependency.BuiltObject );
					return builtObject;
				} );


			var info = context.BuildObject( 1 );
			Assert.IsNotNull( info );
			info.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, info.ObjectBuildingState );
			Assert.IsNotNull( info.BuiltObject );
			Assert.IsInstanceOfType( info.BuiltObject, typeof( string ) );
			Assert.AreEqual( "1-0a-0a", (string) info.BuiltObject );

			var obpOverrides = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( null, fixedObjects2 );
			var overrideContext = new ObjectContext<int>( context, obpOverrides, _taskRunner );

			var info2 = overrideContext.BuildObject( 1 );
			Assert.IsNotNull( info2 );
			info2.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, info2.ObjectBuildingState );
			Assert.IsNotNull( info2.BuiltObject );
			Assert.IsInstanceOfType( info2.BuiltObject, typeof( string ) );
			Assert.AreEqual( "1-0a-0b", (string) info2.BuiltObject );
		}

		#endregion

		#region Asynchronous discovery

		[TestMethod]
		public void CheckAsynchronousDiscoveryWorks_AsyncModeIsTrue()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = "0";

			var builders = new Dictionary<int, ObjectBuilders.IObjectBuilder<int>>();
			var builder = new CheckAsynchronousDiscoveryWorksObjectBuilder( true );
			builders [ 1 ] = builder;

			var obp = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( builders, fixedObjects );
			var context = new ObjectContext<int>( null, obp, _taskRunner );

			var info = context.BuildObject( 1 );
			Assert.IsNotNull( info );
			Assert.AreEqual( ObjectBuildingStates.Starting, info.ObjectBuildingState );
			info.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, info.ObjectBuildingState );
			Assert.IsInstanceOfType( info.BuiltObject, typeof( string ) );
			Assert.AreEqual( "Wrapped 0", (string) info.BuiltObject );
			Assert.IsTrue( builder.DiscoveryThreadID.HasValue );
			Assert.AreNotEqual( System.Threading.Thread.CurrentThread.ManagedThreadId, builder.DiscoveryThreadID.Value );
		}

		[TestMethod]
		public void CheckAsynchronousDiscoveryWorks_AsyncModeIsFalse()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = "0";

			var builders = new Dictionary<int, ObjectBuilders.IObjectBuilder<int>>();
			var builder = new CheckAsynchronousDiscoveryWorksObjectBuilder( false );
			builders [ 1 ] = builder;

			var obp = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( builders, fixedObjects );
			var context = new ObjectContext<int>( null, obp, _taskRunner );

			var info = context.BuildObject( 1 );
			Assert.IsNotNull( info );
			Assert.AreEqual( ObjectBuildingStates.DependenciesKnown, info.ObjectBuildingState );
			info.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, info.ObjectBuildingState );
			Assert.IsInstanceOfType( info.BuiltObject, typeof( string ) );
			Assert.AreEqual( "Wrapped 0", (string) info.BuiltObject );
			Assert.IsTrue( builder.DiscoveryThreadID.HasValue );
			Assert.AreEqual( System.Threading.Thread.CurrentThread.ManagedThreadId, builder.DiscoveryThreadID.Value );
		}

		private class CheckAsynchronousDiscoveryWorksObjectBuilder : ObjectBuilders.IObjectBuilder<int>,
			ObjectBuilders.IObjectBuilderAsynchronousDependencies<int>
		{
			public int? DiscoveryThreadID { get; private set; }
			public bool AsyncDiscoveryMode { get; private set; }

			public CheckAsynchronousDiscoveryWorksObjectBuilder( bool asyncDiscoveryMode )
			{
				this.AsyncDiscoveryMode = asyncDiscoveryMode;
			}

			public ObjectBuilders.IDependencySet<int> GetDependencies( ObjectBuilders.IObjectBuilderGetDependenciesContext<int> context, int address )
			{
				System.Threading.Thread.Sleep( 50 );
				this.DiscoveryThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;

				var dependencies = new ObjectBuilders.DependencySet<int>();
				dependencies.AddDependency( "data", 0 );
				return dependencies;
			}

			public object BuildObject( ObjectBuilders.IObjectBuilderBuildObjectContext<int> context, int address, ObjectBuilders.IBuiltDependencies<int> dependencies )
			{
				if( !dependencies.TryGetDependency( "data", out var dependency ) )
					throw new InvalidOperationException( "No dependency called data found" );

				var d = (string) dependency.BuiltObject;
				return "Wrapped " + d;
			}

			public bool GetDependenciesAsynchronously( int address )
			{
				return this.AsyncDiscoveryMode;
			}

			public ObjectBuilders.IDependencySet<int> GetAdditionalDependencies( ObjectBuilders.IObjectBuilderGetAdditionalDependenciesContext<int> context, int address, ObjectBuilders.IBuiltDependencies<int> dependencies )
			{
				return null;
			}
		}

		#endregion

		#region Recursive Dependencies

		[TestMethod]
		public void RecursiveDependencies_Standard()
		{
			var fixedObjects = new Dictionary<int, object>();
			fixedObjects [ 0 ] = 1;
			fixedObjects [ 1 ] = "Object 1";
			fixedObjects [ 2 ] = "Object 2";

			var builders = new Dictionary<int, ObjectBuilders.IObjectBuilder<int>>();
			var builder = new RecursiveDependenciesObjectBuilder();
			builders [ 3 ] = builder;

			var obp = new ObjectBuilders.Providers.FixedObjectBuilderProvider<int>( builders, fixedObjects );
			var context = new ObjectContext<int>( null, obp, _taskRunner );

			var info = context.BuildObject( 3 );
			Assert.IsNotNull( info );
			info.ObjectBuiltOrFailureWaitHandle.WaitOne();
			Assert.AreEqual( ObjectBuildingStates.ObjectBuilt, info.ObjectBuildingState );

			Assert.IsInstanceOfType( info.BuiltObject, typeof( string ) );
			Assert.AreEqual( "Object 1", (string) info.BuiltObject );
		}

		private class RecursiveDependenciesObjectBuilder : ObjectBuilders.IObjectBuilder<int>
		{
			public ObjectBuilders.IDependencySet<int> GetDependencies( ObjectBuilders.IObjectBuilderGetDependenciesContext<int> context, int address )
			{
				var dependencies = new ObjectBuilders.DependencySet<int>();
				dependencies.AddDependency( "redirect", 0 );
				dependencies.RecursiveMode = true;
				return dependencies;
			}

			public ObjectBuilders.IDependencySet<int> GetAdditionalDependencies( ObjectBuilders.IObjectBuilderGetAdditionalDependenciesContext<int> context, int address, ObjectBuilders.IBuiltDependencies<int> dependencies )
			{
				if( !dependencies.TryGetDependency( "redirect", out var redirectDependency ) )
					throw new InvalidOperationException( "No redirect value found" );

				var newAddress = (int) redirectDependency.BuiltObject;

				var additionalDependencies = new ObjectBuilders.DependencySet<int>();
				additionalDependencies.AddDependency( "data", newAddress );
				additionalDependencies.RecursiveMode = false;
				return additionalDependencies;
			}

			public object BuildObject( ObjectBuilders.IObjectBuilderBuildObjectContext<int> context, int address, ObjectBuilders.IBuiltDependencies<int> dependencies )
			{
				if( !dependencies.TryGetDependency( "data", out var dataDependency ) )
					throw new InvalidOperationException();

				return dataDependency.BuiltObject;
			}
		}

        #endregion

        #region Bypassing Task functionality

		[TestMethod]
		public void BypassingTaskFunctionality_Success()
        {
			var obp = new ObjectBuilders.Providers.FunctionBasedObjectBuilderProvider<int>((address) => {
				return new ObjectBuilders.FunctionBasedObjectBuilder<int>(
					(context, address) => { return new ObjectBuilders.DependencySet<int>(); },
					(context, address, builtDependencies) => { return null; },
					(context, address, dependencies) => throw new Exception("Don't call me")
					);
			});

			var taskRunner = new Tasks.FunctionBasedTaskRunner((task) => {
				var objectBuildingTask = task as Tasks.IObjectBuildingTask<int>;
				if (objectBuildingTask == null)
					throw new InvalidOperationException("Er....");

				objectBuildingTask.SetResult(objectBuildingTask.Address * 2);
			});

			var objectContext = new ObjectContext<int>(null, obp, taskRunner);

			var obj = objectContext.BuildObject(5);
			Assert.IsTrue(obj.ObjectBuiltOrFailureWaitHandle.WaitOne());
			Assert.AreEqual(ObjectBuildingStates.ObjectBuilt, obj.ObjectBuildingState);
			Assert.AreEqual(10, obj.BuiltObject);
        }

		[TestMethod]
		public void BypassingTaskFunctionality_Failure()
		{
			var exception = new Exception("Raaaaaaaaaaa");

			var obp = new ObjectBuilders.Providers.FunctionBasedObjectBuilderProvider<int>((address) => {
				return new ObjectBuilders.FunctionBasedObjectBuilder<int>(
					(context, address) => { return new ObjectBuilders.DependencySet<int>(); },
					(context, address, builtDependencies) => { return null; },
					(context, address, dependencies) => throw new Exception("Don't call me")
					);
			});

			var taskRunner = new Tasks.FunctionBasedTaskRunner((task) => {
				var objectBuildingTask = task as Tasks.IObjectBuildingTask<int>;
				if (objectBuildingTask == null)
					throw new InvalidOperationException("Er....");

				objectBuildingTask.SetException(exception);
			});

			var objectContext = new ObjectContext<int>(null, obp, taskRunner);

			var obj = objectContext.BuildObject(5);
			Assert.IsTrue(obj.ObjectBuiltOrFailureWaitHandle.WaitOne());
			Assert.AreEqual(ObjectBuildingStates.Failure, obj.ObjectBuildingState);
			Assert.AreSame(exception, obj.Exception);
		}

		#endregion
	}
}
