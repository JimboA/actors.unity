﻿//  Project : ecs
// Contacts : Pix - ask@pixeye.games

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;


namespace Pixeye.Actors
{
  [Il2CppSetOption(Option.NullChecks | Option.ArrayBoundsChecks | Option.DivideByZeroChecks, false)]
  public static unsafe partial class Entity
  {
    public const bool Pooled = true;

#if !ACTORS_TAGS_0
    static readonly int sizeBufferTags = UnsafeUtility.SizeOf<CacheTags>();
#endif
    static readonly int sizeEntityCache = UnsafeUtility.SizeOf<CacheEntity>();

    public static Transform[] Transforms;

#if !ACTORS_TAGS_0
    public static CacheTags* Tags;
#endif

    public static CacheEntity* entities;

    internal static int lengthTotal;
    internal static int[,] Generations;
    internal static int[,] GenerationsInstant;

    public static ents alive;


    //===============================//
    // Initialize 
    //===============================//

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
#endif
    internal static void Start()
    {
      var t = Resources.Load<TextAsset>("SettingsFramework");
      if (t != null)
        JsonUtility.FromJsonOverwrite(t.text, Kernel.Settings);

      Kernel.Settings.SizeGenerations = Kernel.Settings.SizeComponents / 32;

      lengthTotal = Kernel.Settings.SizeEntities;
      Generations = new int[Kernel.Settings.SizeEntities, Kernel.Settings.SizeGenerations];
      GenerationsInstant = new int[Kernel.Settings.SizeEntities, Kernel.Settings.SizeGenerations];


      Transforms = new Transform[Kernel.Settings.SizeEntities];

      entities = (CacheEntity*) UnmanagedMemory.Alloc(sizeEntityCache * Kernel.Settings.SizeEntities);
#if !ACTORS_TAGS_0
      Tags = (CacheTags*) UnmanagedMemory.Alloc(sizeBufferTags * Kernel.Settings.SizeEntities);
#endif


      for (var i = 0; i < Kernel.Settings.SizeEntities; i++)
      {
#if !ACTORS_TAGS_0
        Tags[i] = new CacheTags();
#endif
        entities[i] = new CacheEntity(6);
      }

      alive = new ents(Kernel.Settings.SizeEntities);
    }

    // Use for other libraries
    public static int GetLiveEntities()
    {
      return alive.length;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Initialize(int id, byte age, bool isPooled = false, bool isNested = false)
    {
      if (id >= lengthTotal)
      {
        var l = id << 1;
        HelperArray.ResizeInt(ref Generations, l, Kernel.Settings.SizeGenerations);
        HelperArray.ResizeInt(ref GenerationsInstant, l, Kernel.Settings.SizeGenerations);
        Array.Resize(ref Transforms, l);

        entities = (CacheEntity*) UnmanagedMemory.ReAlloc(entities, sizeEntityCache * l);
#if !ACTORS_TAGS_0
        Tags = (CacheTags*) UnmanagedMemory.ReAlloc(Tags, sizeBufferTags * l);
#endif

        for (var i = lengthTotal; i < l; i++)
        {
#if !ACTORS_TAGS_0
          Tags[i] = new CacheTags();
#endif
          entities[i] = new CacheEntity(5);
        }

        lengthTotal = l;
      }

      entities[id].componentsAmount = 0;

      var ptrCache = &entities[id];
      ptrCache->age = age;
      ptrCache->isNested = isNested;
      ptrCache->isPooled = isPooled;
 
      ptrCache->isDirty = true;
      ptrCache->isAlive = true;

      
      ent e;
      e.id = id;
      e.age = age;
      alive.Add(e);
    }


    //===============================//
    // Naming
    //===============================//
    static readonly FastString fstr = new FastString(500);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void RenameGameobject(this ent entity)
    {
      if (!Kernel.Settings.DebugNames) return;
      var tr = Transforms[entity.id];
      if (tr != null)
      {
        var name = tr.name;
        var index = tr.name.LastIndexOf(':');
        if (index > -1) name = tr.name.Remove(0, index + 1);
        var id = entity.id;

        name = name.Trim();
        fstr.Clear();
        fstr.Append($"{id.ToString().PadLeft(4, '0')}: ");


        // {
        fstr.Append("[ ");
        for (var j = 0; j < entities[entity.id].componentsAmount; j++)
        {
          var storage = Storage.All[entities[entity.id].componentsIds[j]];
          var lex = j < entities[entity.id].componentsAmount - 1 ? " " : "";
          fstr.Append($"{storage.GetComponentType().Name.Remove(0, 9)}{lex}");
        }

        fstr.Append(" ]: ");
        //}

        fstr.Append(name);

        tr.name = fstr.ToString();
      }
    }


    //===============================//
    // Add & Set
    //===============================//

    /// <summary>
    /// Used in Models and Actors for setting up components to Storage. Doesn't send the component to systems.
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Set<T>(in this ent entity)
    {
      var id = entity.id;

#if UNITY_EDITOR
      if (entity.id == 0)
      {
        Kernel.Debugger.Log(LogType.NULL_ENTITY, entity, typeof(T).Name);
        return ref Storage<T>.Get(id);
      }
#endif


      if (id >= Storage<T>.components.Length)
        Array.Resize(ref Storage<T>.components, id << 1);

      entities[id].Add(Storage<T>.componentId);
      GenerationsInstant[id, Storage<T>.Generation] |= Storage<T>.ComponentMask;

      ref var val = ref Storage<T>.components[id];

#if !ACTORS_COMPONENTS_STRUCTS
      if (val == null)
        val = Storage<T>.Instance.Create();
#endif

      return ref val;
    }

    /// <summary>
    /// Used in Models and Actors for setting up components to Storage. Doesn't send the component to systems.
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Set<T>(in this ent entity, T component)
    {
      var id = entity.id;

      if (id >= Storage<T>.components.Length)
        Array.Resize(ref Storage<T>.components, id << 1);

      entities[id].Add(Storage<T>.componentId);
      GenerationsInstant[id, Storage<T>.Generation] |= Storage<T>.ComponentMask;

      ref var componentInStorage = ref Storage<T>.components[id];
      componentInStorage = component;
    }

    /// <summary>
    /// Deploy entity components to systems.
    /// Note: components from Models and Actors are deployed automatically.
    /// </summary>
    /// <param name="entity"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void Deploy(in this ent entity)
    {
      EntityOperations.Set(entity, -1, EntityOperations.Action.Activate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T AddGet<T>(in this ent entity)
    {
      var id = entity.id;


#if UNITY_EDITOR

      if (!entity.exist)
      {
        Kernel.Debugger.Log(LogType.NOT_ACTIVE, entity, typeof(T).Name);
        return ref Storage<T>.Get(id);
      }

#endif

      if (id >= Storage<T>.components.Length)
        Array.Resize(ref Storage<T>.components, id << 1);


      if ((GenerationsInstant[id, Storage<T>.Generation] & Storage<T>.ComponentMask) != Storage<T>.ComponentMask)
      {
        entities[id].Add(Storage<T>.componentId);
        GenerationsInstant[id, Storage<T>.Generation] |= Storage<T>.ComponentMask;

        if (!entities[id].isDirty)
        {
          EntityOperations.Set(entity, Storage<T>.componentId, EntityOperations.Action.Add);
        }
      }

      ref var val = ref Storage<T>.components[id];

#if !ACTORS_COMPONENTS_STRUCTS
      if (val == null)
        val = Storage<T>.Instance.Create();
#endif

      return ref val;
    }

    /// <summary>
    /// Attach component to an entity and systems.
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref T Add<T>(in this ent entity)
    {
      var id = entity.id;


      if (id >= Storage<T>.components.Length)
        Array.Resize(ref Storage<T>.components, id << 1);


      ref var val = ref Storage<T>.components[id];

#if !ACTORS_COMPONENTS_STRUCTS
      if (val == null)
        val = Storage<T>.Instance.Create();
#endif

#if UNITY_EDITOR

      if (!entity.exist)
      {
        Kernel.Debugger.Log(LogType.NOT_ACTIVE, entity, typeof(T).Name);
        return ref Storage<T>.Get(id);
      }

      if ((GenerationsInstant[id, Storage<T>.Generation] & Storage<T>.ComponentMask) == Storage<T>.ComponentMask)
      {
        Kernel.Debugger.Log(LogType.ALREADY_HAVE, entity, typeof(T).Name);
        return ref val;
      }
#endif


      entities[id].Add(Storage<T>.componentId);
      GenerationsInstant[id, Storage<T>.Generation] |= Storage<T>.ComponentMask;

      if (!entities[id].isDirty)
      {
        EntityOperations.Set(entity, Storage<T>.componentId, EntityOperations.Action.Add);
      }

      return ref val;
    }


    /// <summary>
    /// Attach component to an entity and systems.
    /// </summary>
    /// <param name="entity"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add<T>(in this ent entity, T component)
    {
      var id = entity.id;

#if UNITY_EDITOR

      if (!entity.exist)
      {
        Kernel.Debugger.Log(LogType.NOT_ACTIVE, entity, typeof(T).Name);
        return;
      }

      if ((Generations[id, Storage<T>.Generation] & Storage<T>.ComponentMask) == Storage<T>.ComponentMask)
      {
        Kernel.Debugger.Log(LogType.ALREADY_HAVE, entity, typeof(T).Name);
        return;
      }
#endif

      ref var componentInStorage = ref Storage<T>.components[id];
      componentInStorage = component;


      entities[id].Add(Storage<T>.componentId);
      GenerationsInstant[id, Storage<T>.Generation] |= Storage<T>.ComponentMask;

      if (!entities[id].isDirty)
      {
        EntityOperations.Set(entity, Storage<T>.componentId, EntityOperations.Action.Add);
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Remove<T>(in this ent entity)
    {
#if UNITY_EDITOR
      if (!entity.exist)
      {
        Kernel.Debugger.Log(LogType.REMOVE_NON_EXISTANT, entity, typeof(T).Name);
        return;
      }
#endif
      var id = entity.id;
      if ((GenerationsInstant[id, Storage<T>.Generation] & Storage<T>.ComponentMask) == Storage<T>.ComponentMask)
      {
        GenerationsInstant[id, Storage<T>.Generation] &= ~Storage<T>.ComponentMask;
        EntityOperations.Set(entity, Storage<T>.componentId, EntityOperations.Action.Remove);
      }
    }


    internal static void Dispose()
    {
      for (var i = 0; i < lengthTotal; i++)
        Marshal.FreeHGlobal((IntPtr) entities[i].componentsIds);

      UnmanagedMemory.Cleanup();
    }
  }
}