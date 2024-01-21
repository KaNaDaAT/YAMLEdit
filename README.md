# YAMLEdit

YAMLEdit is a lightweight library designed to simplify the reading and editing of YAML files in C#. With YAMLEdit, you can effortlessly read, modify, and save YAML data using a straightforward and intuitive API.

## Features

- **Easy Reading**: Quickly read YAML files with a simple and concise syntax.

- **Dynamic Editing**: Modify YAML data dynamically using the provided methods.

- **Type Safety**: Ensure type safety when getting and setting values from YAML files.

- **Effortless Saving**: Save your changes back to the YAML file with a single command.

## Getting Started

1. Install the YAMLEdit NuGet package in your C# project.
   
   ```bash
   dotnet add package YAMLEdit
   ```

2. Import the YAMLEdit namespace in your C# file.

3. Start using the library in your code.
   
   ```c#
   ConfigFile config = new ConfigFile(pathToYaml);
   config.Set("Persons.1.Name", "KaNaDaAT");
   config.Set("Persons.1.Age", 21);
   config.Set("Persons.1.Age", 22); // Overwrite the value
   int age = config.Get<int>("Persons.1.Age"); // 22
   config.Save(); // actually writes the data
   ```

## Examples

**Example 1: Reading YAML File**

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

**Example 2: Updating YAML Data**

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

## API Reference

### ConfigFile Class

**Constructor:** `new ConfigFile(string filePath, bool autoLoad = true)`: Creates a new instance of the ConfigFile class with the specified YAML file path.

**Set Method:** `Set(string key, object value)`: Sets the value for the specified key in the YAML file.

**Get Method:** `Get<T>(string key)`: Retrieves the value for the specified key from the YAML file, ensuring type safety.

**Save Method:** `Save()`: Writes the changes back to the YAML file.

**Load Method:** `Load()`: Loads the YAML file.

## Contribution

Feel free to contribute to the development of YAMLEdit. For bug reports, feature requests, or other contributions, please open an issue or submit a pull request
