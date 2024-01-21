# YAMLEdit

YAMLEdit is a lightweight library designed to simplify the reading and editing of YAML files in C#. With YAMLEdit, you can effortlessly read, modify, and save YAML data using a straightforward and intuitive API.

## Features

- **Easy Reading**: Quickly read YAML files with a simple and concise syntax.

- **Dynamic Editing**: Modify YAML data dynamically using the provided methods.

- **Type Safety**: Ensure type safety when getting and setting values from YAML files.

- **Effortless Saving**: Save your changes back to the YAML file with a single command.

## Getting Started
UNDER DEVELOPMENT

```c#
ConfigFile config = new ConfigFile(pathToYaml);
config.Set("Persons.1.Name", "KaNaDaAT");
config.Set("Persons.1.Age", 21);
config.Set("Persons.1.Age", 22); // Overwrite the value
int age = config.Get<int>("Persons.1.Age"); // 22
config.Save(); // actually writes the data
```

## Paths/Keys

In YAMLEdit, paths are used to navigate through the YAML structure and access specific nodes or elements within the configuration. Understanding how paths are constructed is crucial for effectively retrieving and modifying data. Paths consist of keys separated by dots (`.`) to represent the hierarchy in the YAML structure.

### Dot Notation

- **Standard Keys**: Standard keys are accessed using dot notation. For example, `Rooms.ROOM0` accesses the value associated with the key "ROOM0" within the "Rooms" node.

### Colon Notation for Sequences

- **Accessing Sequences**: To access elements within sequences, colon notation (`:`) is employed. For example, `Rooms.Special.:0` refers to the first element within the "Special" sequence in the "Rooms" node.

- **Nested Sequences**: When dealing with nested sequences, additional colons are used. For instance, `Rooms.Special.:1.:0` accesses the first element within the second sequence in the "Special" sequence.

### Case Sensitivity

YAMLEdit is case-sensitive when it comes to key matching. Ensure that the keys specified in the path exactly match the casing used in the YAML file. For example, `Rooms.special` and `Rooms.Special` would refer to different nodes.

## Examples

### **Example 1: Reading YAML File**

```c#
ConfigFile config = new ConfigFile("example.yaml");

string name = config.Get<string>("Person.Name");
int age = config.Get<int>("Person.Age");

Console.WriteLine($"Name: {name}, Age: {age}");
```

Example 1 - YAML File

```yaml
Person:
  Name: John Doe
  Age: 30
  Sex: Male
```

### **Example 2: Updating YAML Data**

```c#
ConfigFile config = new ConfigFile("example.yaml");

config.Set("Person.Name", "Jane Smith");
config.Set("Person.Age", 32);

config.Save();
```

Example 2 - Updated YAML File

```yaml
Person:
  Name: Jane Smith
  Age: 32
  Sex: Male
```

### **Example 3: Sequences**

**Accessing the entire "Rooms" node**

```c#
config.GetNode("Rooms");
// Output: [ ROOM0, { { Special, [ NORMAL0, [ DEEP0, DEEP1 ], NORMAL1, NORMAL2, [ [ DEEP2, DEEP3 ] ] ] } }, ROOM1, ROOM2 ]
```

**Accessing specific elements within the "Rooms" node**

```c#
config.GetNode("Rooms.ROOM0");
// Output: ROOM0
config.GetNode("Rooms.Special");
// Output: [ NORMAL0, [ DEEP0, DEEP1 ], NORMAL1, NORMAL2, [ [ DEEP2, DEEP3 ] ] ]
```

**Accessing via index within the "Rooms" node**

```c#
Console.WriteLine(config.GetNode("Rooms.:0"));
Console.WriteLine(config.GetNode("Rooms.0"));
// Output: ROOM0
```

While both work the first is preffered.

**Accessing inside 'Special'**

```c#
Console.WriteLine(config.GetNode("Rooms.Special.:0"));
// Output: NORMAL0
Console.WriteLine(config.GetNode("Rooms.Special.:1"));
// Output: [ DEEP0, DEEP1 ]
Console.WriteLine(config.GetNode("Rooms.Special.:2"));
// Output: NORMAL2
```

**Accessing deep sequences**

```c#
Console.WriteLine(config.GetNode("Rooms.Special.:0:.DEEP0"));
Console.WriteLine(config.GetNode("Rooms.Special.:0:.:0"));
// Output: DEEP0

Console.WriteLine(config.GetNode("Rooms.Special.:1:"));
// Output: [ [ DEEP2, DEEP3 ] ]
Console.WriteLine(config.GetNode("Rooms.Special.:1:.:0"));
// Output: [ DEEP2, DEEP3 ]
Console.WriteLine(config.GetNode("Rooms.Special.:1:.:0:.:1"));
// Output: DEEP3
```

- `Rooms.:0`: Accesses the first element of the "Rooms" sequence using dot notation.
- `Rooms.Special.:0.:0`: Accesses the first element of the "Special" sequence within "Rooms," then accesses the first element of the nested sequence. The colon notation is used to indicate sequence elements.
- The difference between `:0:` and `:0` is that `:0:` represents accessing the element in a sequence, while `:0` represents accessing a property or element with the name "0" directly. In the context of this example, `.:0` is used to access elements in the "Rooms" sequence, while `.:0:` is used for nested sequences within "Special."

Example 3  - Rooms YAML File

```yaml
Rooms:
- ROOM0
- Special:
  - NORMAL0
  - - DEEP0
    - DEEP1
  - NORMAL1
  - NORMAL2
  - - - DEEP2
      - DEEP3
- ROOM1
- ROOM2
```

# 

## API Reference

### ConfigFile Class

**Constructor:** `new ConfigFile(string filePath, bool autoLoad = true)`: Creates a new instance of the ConfigFile class with the specified YAML file path.

**Set Method:** `Set(string key, object value)`: Sets the value for the specified key in the YAML file.

**Get Method:** `Get<T>(string key)`: Retrieves the value for the specified key from the YAML file, ensuring type safety.

**Save Method:** `Save()`: Writes the changes back to the YAML file.

**Load Method:** `Load()`: Loads the YAML file.

**Node Method:** `GetNode(string key)`: Retrieves the YAMLNode for the specified key from the YAML file.

## Contribution

Feel free to contribute to the development of YAMLEdit. For bug reports, feature requests, or other contributions, please open an issue or submit a pull request
