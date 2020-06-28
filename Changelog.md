## Changelog 
### Actors-2020-06-03
⭐ **Changed** 
* [📘 Improving Actors + Unity Job](https://github.com/PixeyeHQ/actors.unity/issues/45)
* [📘 Add Layer.Get`<T>`](https://github.com/PixeyeHQ/actors.unity/issues/47)
* [📘 Make scenes work from asset bundles in new version of Actors](https://github.com/PixeyeHQ/actors.unity/issues/51)

🛠️ **Fixed** 
* [🐞 ApplicationIsQuitting is not working](https://github.com/PixeyeHQ/actors.unity/issues/46)
* [🐞 Setup of Monocached is triggered twice](https://github.com/PixeyeHQ/actors.unity/issues/49)
* [🐞 PostHandleFrameworkPatch.cs(128,1): error CS1022](https://github.com/PixeyeHQ/actors.unity/issues/50)
* [🐞 Object Pooling for Actors on the scene doesn't work properly](https://github.com/PixeyeHQ/actors.unity/issues/54)

⚠️ **IMPORTANT** ⚠️  
If you have Actors on the scene that must be added to the pool system use field below to add **prefab name** of that object. If you don't want to use pooling system with
the object just leave a blank field.
[![Actor](https://i.gyazo.com/3ec4e26a0b9638016834f5fcd36a36b4.png)](https://gyazo.com/3ec4e26a0b9638016834f5fcd36a36b4)

### Actors-2020-06-02
🛠️ **Fixed** 
* [🐞 Pooled gameobject of entity gets null reference exception.](https://github.com/PixeyeHQ/actors.unity/issues/36)
* [🐞 Pooled Actors won't initialize after re-activating.](https://github.com/PixeyeHQ/actors.unity/issues/37)
* [🐞 Childs of entities are not initialized.](https://github.com/PixeyeHQ/actors.unity/issues/38)
* [🐞 Groups are not cleaned when a scene is reloaded.](https://github.com/PixeyeHQ/actors.unity/issues/39)
* [🐞 Actors continue to think that they are on the old layer when using `entity.MoveTo<T>`](https://github.com/PixeyeHQ/actors.unity/issues/40)
* [🐞 Unloading scene in the inspector and hitting play breaks the editor.](https://github.com/PixeyeHQ/actors.unity/issues/41)
* [🐞 Tools->Actors->Update Actors doesn't work in the Unity 2019.4](https://github.com/PixeyeHQ/actors.unity/issues/43)
* [🐞 Updating entities from the Added/Removed Ecs events do not work correctly.](https://github.com/PixeyeHQ/actors.unity/issues/44)

### Actors-2020-06 | Codename: Bluefin Tuna
⚠️ **IMPORTANT** ⚠️   
The new version of Actors **is not compatible** with previous versions. If you are using ***Actors-2020-04-30***  or earlier version in the production cycle, please change your manifest link to: ```"com.pixeye.ecs": "https://github.com/PixeyeHQ/actors.unity.git#actors-2020-04-30",```

The framework was rewritten so there is no reason to write down all changes. Look [README](https://github.com/PixeyeHQ/actors.unity/blob/develop/README.md).

If you want to migrate to the new version of Actors checkout the [MIGRATION GUIDE](https://github.com/PixeyeHQ/actors.unity/wiki/Migration-to-Actors-2020.06).


