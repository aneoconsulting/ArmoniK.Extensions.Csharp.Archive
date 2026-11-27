// This file is part of the ArmoniK project
// 
// Copyright (C) ANEO, 2021-2023. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace ArmoniK.DevelopmentKit.Worker.DLLWorker;

internal class AddOnAssemblyLoadContext : AssemblyLoadContext
{
  /// <summary>
  ///   Simple names of the assemblies the host resolves for the default context: the worker's own dependencies and,
  ///   when the worker is framework-dependent, the shared frameworks. A guest must use these instead of its own copies,
  ///   so that the types it exchanges with the worker (IConfiguration, ILoggerFactory, the worker interfaces) are the same.
  /// </summary>
  private static readonly HashSet<string> SharedAssemblies = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? string.Empty).Split(Path.PathSeparator,
                                                                                                                                                 StringSplitOptions
                                                                                                                                                   .RemoveEmptyEntries)
                                                                                                                                          .Select(Path
                                                                                                                                                    .GetFileNameWithoutExtension)
                                                                                                                                          .ToHashSet(StringComparer
                                                                                                                                                       .OrdinalIgnoreCase);

  private readonly AssemblyDependencyResolver resolver_;

  public AddOnAssemblyLoadContext()
    : base(Guid.NewGuid()
               .ToString(),
           true)
  {
  }

  public AddOnAssemblyLoadContext(string mainAssemblyToLoadPath)
    : base(Guid.NewGuid()
               .ToString(),
           true)
    => resolver_ = new AssemblyDependencyResolver(mainAssemblyToLoadPath);

  protected override Assembly Load(AssemblyName name)
  {
    if (name.Name is not null && SharedAssemblies.Contains(name.Name))
    {
      return null;
    }

    var assemblyPath = resolver_.ResolveAssemblyToPath(name);

    return assemblyPath != null
             ? LoadFromAssemblyPath(assemblyPath)
             : null;
  }

  /// <summary>Allows derived class to load an unmanaged library by name.</summary>
  /// <param name="unmanagedDllName">
  ///   Name of the unmanaged library. Typically this is the filename without its path or
  ///   extensions.
  /// </param>
  /// <returns>A handle to the loaded library, or <see cref="F:System.IntPtr.Zero" />.</returns>
  protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
  {
    var assemblyPath = resolver_.ResolveUnmanagedDllToPath(unmanagedDllName);
    return assemblyPath != null
             ? LoadUnmanagedDllFromPath(assemblyPath)
             : IntPtr.Zero;
  }
}
