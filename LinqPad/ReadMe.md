# Marble Diagram generator from LinqPad

Given a text file `Marble_Take3.txt` like

```text
{source     }-0-1-2-3-4-]
{           } | | | 
{result     }-0-1-2-]
```

when processed by `MarbleDiagramGenerator.linq` with a command like

```shell
> lprun .\LinqPad\MarbleDiagramGenerator.linq .\sampleInputFiles\Marble_Take3.txt
```

An image like this is produced.

![Marble diagram of the Take(3) operator applied to an Interval sequence](https://raw.githubusercontent.com/LeeCampbell/MarbleDiagram/master/Resources/Marble_Take3.png)

## How to use it

* Download and install [LinqPad](https://www.linqpad.net/)
* *Optional* put the LinqPad install dir `C:\Program Files\LINQPad5` in your path
* run `lprun .\MarbleDiagramGenerator.linq Your_Input_File.txt`
  * Replacing `Your_Input_File.txt` with the path to your input file
  * Ensuring  MarbleDiagramGenerator.linq is either in the current directory, or provide a fully qualified path to it
  * Ensuring lprun.exe is in your PATH, or has a fully qualified path  

### Sample commands

Each of these sampel assume you are running from this directory.

DOS Command Prompt:

```
C:\GitHub\MarbleDiagram\LinqPad>"C:\Program Files\LINQPad5\lprun" MarbleDiagramGenerator.linq ..\SampleInputFiles\Marble_0to4.txt
```

Powershell Prompt:

```PS
& "C:\Program Files\LINQPad5\lprun.exe" MarbleDiagramGenerator.linq ..\SampleInputFiles\Marble_0to4.txt
```

or if you have `lprun` on your path, then just (in either DOS Command Prompt or Powershell)

```PS
lprun MarbleDiagramGenerator.linq ..\SampleInputFiles\Marble_0to4.txt
```

Each of these will create a `Marble_0to4.png` file in _./sampleInputFiles/_ directory (i.e. next to the source file).


The image will be created and saved to the same location as the input file.
In this case it would create and image at `C:\Users\lee.campbell\Documents\MarbleDiagrams\Marble_0to4.png`

## Input format

The `MarbleDiagramGenerator.linq` file is a LinqPad script file that allows you to generate Marble diagrams in png form. It supports simple, small diagrams which I mainly use for adding to presentation slide decks.

I have create a mildly readable format that the input text file should adhere to. 
The format was more an evolution than a design.

At the more complex end of the tool's ability is to take a file like this and produce and output.

```text
{ints      }-1-------2-------3-]
{          } |       |       |
{chars     }-x-y-z-] |       |
{          } | | |  -x-y-z-] |
{          } | | |   | | |  -x-y-z-]
{          } | | |   | | |   | | | 
{result    }-(1x)-(1y)-(1z)---(2x)-(2y)-(2z)---(3x)-(3y)-(3z)-]
```

*Output:*

![Marble diagram of the Take(3) operator applied to an Interval sequence](https://raw.githubusercontent.com/LeeCampbell/MarbleDiagram/master/Resources/Marble_CartesianProduct.png)

I have found that diagrams that are larger and more complex than this tend to loose meaning and should be broken down into more simple concepts.
This allows you to focus on on breaking problems down into smaller parts.
It also has the side effect of keeping this tool simple.

## Format

The format is broken in to lines.
Each line is made of the following parts :

1. Sequence Name wrapped by curly braces e.g. `{s1}`
2. unit of time represented by a dash i.e. `-`
3. single character value e.g. `1` or `a`
4. multi value character wrapped by parenthesis - e.g. `(foo)`
5. erroneous sequence termination a.k.a OnError represented by the reserved character 'x' - i.e. `x` 
6. natural sequence termination a.k.a OnComplete represented by the reserved character ']' - i.e. `]`
7. Pipe representing a fall through of a value from an above sequence - e.g. `|`
