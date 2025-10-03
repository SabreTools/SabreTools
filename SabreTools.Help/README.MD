# SabreTools.Help

This library contains logic to both parse commandlines using typed inputs as well as format and output help text. Formatting and output is context-aware, allowing for different behavior if console outputs are redirected.

All inputs allow for non-flag inputs in both `a=b` and `a b` formats. This allows for greater flexibility for cross-platform support. Flags for each input are fully defined by the implementer, so inputs of the form `-a`, `--a`, `/a`, and `a` are all accepted and can even be mixed. The only restriction is that flags should not end with the `=` character as it may interfere with default parsing.

For an example of a program implementing the library, see [SabreTools](https://github.com/SabreTools/SabreTools).

## Special Types

Below is a list of special classes included in the library and their utility to implementers.

| Class | Description |
| --- | --- |
| `Feature` | Represents an application-level feature that may have its own custom set of supported inputs. Features can also allow for internal processing if preferred by the implementer. A default implementation of argument parsing is included but can be overridden by any implementing class. It is recommended but not required to include defined features in a `FeatureSet` to allow for better flexibility.<br/>An example of a Feature would be something like `Program.exe featurename` where it represents a single type of operation that can be done. In single-operation programs, this can represent the default state of the program internally without needing an external name. |
| `FeatureSet` | Represents a logically-grouped set of functionality, usually scoped to an application. Inputs defined in a feature set do not need to be any specific type, but using `Feature` is recommended. `FeatureSet` also allows implementers to easily print help text to the screen.<br/>An example of a feature set would be including `Program.exe feature1` and `Program.exe feature2`as top-level pieces of functionality. This can allow implementers to have multiple feature sets that depend on execution environment or even other commandline arguments. In single-operation or single-set programs, this can represent the default state of the program internally. |
| `Inputs.UserInput` | Base class used for all supported input classes. Both typed and untyped variants exist. This can be used to define custom input classes for any type, including types already defined by one of the default included classes. The typed version of the base class is recommended, but not mandatory.<br/>`Get` and `TryGet` methods are included to make finding values from child items easier. If new input types are defined by the implementer, it is recommended that extension methods are created to include this functionality. |

## Supported Input Formats

Below is a mapping from default supported types to their respective class names in `SabreTools.Help.Inputs`.

| Type | Class | Notes |
| --- | --- | --- |
| `bool` | `BooleanInput` | Requires either `true` or `false` as the value, e.g. `--flag=true` |
| `bool` | `FlagInput` | Inclusion of this indicates a `true` value, e.g. `--flag` |
| `sbyte` | `Int8Input` | Numeric input bounded to `sbyte.MinValue` to `sbyte.MaxValue`, inclusive |
| `short` | `Int16Input` | Numeric input bounded to `short.MinValue` to `short.MaxValue`, inclusive |
| `int` | `Int32Input` | Numeric input bounded to `int.MinValue` to `int.MaxValue`, inclusive |
| `long` | `Int64Input` | Numeric input bounded to `long.MinValue` to `long.MaxValue`, inclusive |
| `string?` | `StringInput` | Non-repeating generic string input; may be empty |
| `List<string>` | `Int8Input` | Repeating generic string input; may be empty |
| `byte` | `UInt8Input` | Numeric input bounded to `byte.MinValue` to `byte.MaxValue`, inclusive |
| `ushort` | `UInt16Input` | Numeric input bounded to `ushort.MinValue` to `ushort.MaxValue`, inclusive |
| `uint` | `UInt32Input` | Numeric input bounded to `uint.MinValue` to `uint.MaxValue`, inclusive |
| `ulong` | `UInt64Input` | Numeric input bounded to `ulong.MinValue` to `ulong.MaxValue`, inclusive |

## Reference Implementations

Three reference feature implementations are included for common cases. They are defined in the table below.

| Class | Description |
| --- | --- |
| `DefaultHelp` | Outputs either a generic help text or one specific to a feature name included as the second argument |
| `DefaultHelpExtended` | Outputs either a generic help text with extended descriptions or one specific to a feature name included as the second argument |
| `DefaultVersion` | Outputs the version number of the program, derived from the informational version |
