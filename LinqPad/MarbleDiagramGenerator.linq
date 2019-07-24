<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\WPF\PresentationFramework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\wpf\PresentationCore.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xaml.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\wpf\WindowsBase.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\wpf\UIAutomationProvider.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Deployment.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\wpf\PresentationUI.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\wpf\System.Printing.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\wpf\ReachFramework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\wpf\UIAutomationTypes.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Accessibility.dll</Reference>
  <NuGetReference>System.Reactive</NuGetReference>
  <Namespace>System</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Concurrency</Namespace>
  <Namespace>System.Reactive.Disposables</Namespace>
  <Namespace>System.Reactive.Joins</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>System.Reactive.PlatformServices</Namespace>
  <Namespace>System.Reactive.Subjects</Namespace>
  <Namespace>System.Reactive.Threading.Tasks</Namespace>
  <Namespace>System.Windows.Controls</Namespace>
  <Namespace>System.Windows.Media</Namespace>
  <Namespace>System.Windows</Namespace>
  <Namespace>System.Windows.Shapes</Namespace>
  <Namespace>System.Windows.Media.Imaging</Namespace>
</Query>

void Main(string[] args)
{
	//Usage: From the command line call lprun.exe (LinqPad console tool) with this query path and the source marble diagram text file
	// eg.
	// >lprun MarbleDiagramGenerator.linq Marble_Switch.txt
	//You probably can find lprun.exe in C:\Program Files (x86)\LINQPad4
	
	string inputPath;
	if(args==null || args.Length!=1)
	{
		throw new ArgumentException("Requires a single argument of the path to the ascii marble diagram to render as a png");
	}else{
		inputPath = args[0];
	}
	
	var outputImagePath = System.IO.Path.ChangeExtension(inputPath, "png");
	var parsers = new LineParserBase[]{new FallThroughLineParser(), new SequenceLineParser()};
	var input = File.ReadAllText(inputPath);
	var q = from line in input.Split(new string[]{Environment.NewLine},StringSplitOptions.RemoveEmptyEntries)
		    select parsers.Select(p => p.ParseLine(line)).FirstOrDefault(p => p!=null);
	
	var panel = new StackPanel();
	foreach (var element in q)
	{
		panel.Children.Add(element);
	}
	var maxImageWidth = 2200;
	var maxImageHeight = 2000;
	SaveAsPng(panel, maxImageWidth, maxImageHeight, outputImagePath);
}

// Define other methods and classes here
public abstract class LineParserBase
{
	protected static readonly int labelFontSize = 56;
	protected static readonly int labelWidth = 200;
	protected static readonly int valueFontSize = 40;
	protected static readonly int unitWidth = 100;
	protected static readonly int unitHeight = 100;
	protected static readonly Thickness unitMargin = new Thickness(10);
	protected static readonly Brush pastelBlue = new SolidColorBrush(Color.FromRgb(0x64, 0x95, 0xED));
	protected static readonly Brush pastelRed = new SolidColorBrush(Color.FromRgb(0xED, 0x64, 0x95));
	protected static readonly Brush pastelPurple = new SolidColorBrush(Color.FromRgb(0xED, 0x64, 0xED));
	protected static readonly Brush pastel4 = new SolidColorBrush(Color.FromRgb(0x95, 0x64, 0xED));
	protected static readonly Brush pastel5 = new SolidColorBrush(Color.FromRgb(0x64, 0xED, 0x64));

	public abstract FrameworkElement ParseLine(string asciiMarbleDiagramLine);
	
	protected string ParseLabel(string input)
	{
		return input.Replace("{", string.Empty)
					.Replace("}", string.Empty)
					.Trim();
	}
	
	protected FrameworkElement CreateSequenceLabel(string name)
	{
		return new TextBlock(){Text=name, Width=labelWidth, FontSize=labelFontSize, Foreground=new SolidColorBrush(Colors.SteelBlue), FontWeight=FontWeights.Bold, VerticalAlignment=VerticalAlignment.Center, HorizontalAlignment=HorizontalAlignment.Center};
	}
	
	protected FrameworkElement CreateSpace()
	{
		return new Line(){X2=unitWidth, StrokeThickness=0, Margin=unitMargin, VerticalAlignment=VerticalAlignment.Center};
	}
}

public class SequenceLineParser : LineParserBase
{
	private static readonly Brush[] valueFillBrushes = new Brush[]{pastelBlue, pastelRed, pastelPurple, pastel4, pastel5};
	//private static readonly Brush[] valueFillBrushes = new Brush[]{pastelBlue, pastelRed, pastelRed, pastelRed, pastelPurple};
	private int currentValueFillBrushIdx = 0;
	
	public override FrameworkElement ParseLine(string asciiMarbleDiagramLine)
	{
		//Validate the input : Single line, only made up of (label) [-|value]*(x||)
		//(?: ) creates a non matching group. Basically just using () will add what it matches to a group in the results, just noise.
		var regex = new Regex(@"^(?<label>\{.*\})(?<pre>\s|\|)*(?<data>-|\w|\s|\(.+?\))+?(?<terminator>x|X|\])?(?<post>\s|\|)*$");
		var match = regex.Match(asciiMarbleDiagramLine);
		if(!match.Success)
		{
			return null;
		}
		
		var valueFillBrush = valueFillBrushes[currentValueFillBrushIdx++%valueFillBrushes.Length];
		var panel = new StackPanel();
		panel.Orientation = Orientation.Horizontal;
		if(match.Groups["label"].Success)
		{
			var label = ParseLabel(match.Groups["label"].Value);
			panel.Children.Add(CreateSequenceLabel(label));
		}
		else
		{
			panel.Children.Add(CreateSequenceLabel(string.Empty));
		}
		
		foreach (var capture in match.Groups["pre"].Captures.Cast<Capture>())
		{
			FrameworkElement entry;
			if(capture.Value==" ") entry = CreateSpace();
			else entry = CreatePipe();	////if(capture.Value=="|") 
			
			panel.Children.Add(entry);
		}
		
		foreach (var capture in match.Groups["data"].Captures.Cast<Capture>())
		{
			FrameworkElement entry;
			if(capture.Value == "-") entry = CreateBlank();
			else if(capture.Value==" ") entry = CreateSpace();
			else
			{
				var value = capture.Value.Replace("(", string.Empty).Replace(")", string.Empty);
				entry = CreateOnNext(value, valueFillBrush);
			}
			panel.Children.Add(entry);
		}
		if(match.Groups["terminator"].Success)
		{
			if(match.Groups["terminator"].Value == "]")
				panel.Children.Add(CreateOnCompleted());
			else
				panel.Children.Add(CreateOnError());
		}
		foreach (var capture in match.Groups["post"].Captures.Cast<Capture>())
		{
			FrameworkElement entry;
			if(capture.Value==" ") entry = CreateSpace();
			else entry = CreatePipe();	////if(capture.Value=="|") 
			
			panel.Children.Add(entry);
		}
		return panel;	
	}
	
	private FrameworkElement CreateBlank()
	{
		return new Line(){X2=unitWidth, Stroke=new SolidColorBrush(Colors.Black), StrokeThickness=4, Margin=unitMargin, VerticalAlignment=VerticalAlignment.Center, SnapsToDevicePixels=true};
	}
	private FrameworkElement CreatePipe()
	{
		return new Line(){Y2=unitHeight, Stroke=new SolidColorBrush(Colors.Gray), StrokeThickness=4, StrokeEndLineCap=PenLineCap.Round, Margin=new Thickness(unitMargin.Left + (unitWidth/2)-1,unitMargin.Top,unitMargin.Right+(unitWidth/2)-1,unitMargin.Bottom), VerticalAlignment=VerticalAlignment.Center};
	}
	private FrameworkElement CreateOnNext(string value, Brush fillBrush)
	{
		var circle = new Ellipse(){Width = unitWidth, Height=unitHeight, Stroke=new SolidColorBrush(Colors.Black), StrokeThickness=1, Margin=unitMargin, Fill=fillBrush};
		var text = new TextBlock(){Text=value, FontSize=valueFontSize, VerticalAlignment=VerticalAlignment.Center, HorizontalAlignment=HorizontalAlignment.Center, Foreground=new SolidColorBrush(Colors.WhiteSmoke)};
		var grid = new Grid();
		grid.Children.Add(circle);
		grid.Children.Add(text);
		return grid;
	}
	private FrameworkElement CreateOnCompleted()
	{
		return new Line(){Y2=unitHeight, Stroke=new SolidColorBrush(Colors.Black), StrokeThickness=4, StrokeEndLineCap=PenLineCap.Round, Margin=new Thickness(unitMargin.Left + (unitWidth/2)-1,unitMargin.Top,unitMargin.Right+ (unitWidth/2)-1,unitMargin.Bottom), VerticalAlignment=VerticalAlignment.Center};
	}
	private FrameworkElement CreateOnError()
	{
		var a = new Line(){X2 = unitWidth, Y2=unitHeight, Stroke=new SolidColorBrush(Colors.Red), StrokeThickness=4, StrokeEndLineCap=PenLineCap.Round, Margin=unitMargin, VerticalAlignment=VerticalAlignment.Center};
		var b = new Line(){X1 = unitWidth, Y2=unitHeight, Stroke=new SolidColorBrush(Colors.Red), StrokeThickness=4, StrokeEndLineCap=PenLineCap.Round, Margin=unitMargin, VerticalAlignment=VerticalAlignment.Center};
		var grid = new Grid();
		grid.Children.Add(a);
		grid.Children.Add(b);
		return grid;
	}
}

public class FallThroughLineParser : LineParserBase
{
	public override FrameworkElement ParseLine(string asciiMarbleDiagramLine)
	{
		//Validate the input : Single line, only made up of (label) [ ||]*
		//(?: ) creates a non matching group. Basically just using () will add what it matches to a group in the results, just noise.
		var regex = new Regex(@"^(?<label>\{.*\})(?<data>\s|\|)+$");
		var match = regex.Match(asciiMarbleDiagramLine);
		if(!match.Success)
		{
			return null;
		}
		
		var panel = new StackPanel();
		panel.Orientation = Orientation.Horizontal;
		if(match.Groups["label"].Success)
		{
			var label = ParseLabel(match.Groups["label"].Value);
			panel.Children.Add(CreateSequenceLabel(label));
		}
		else
		{
			panel.Children.Add(CreateSequenceLabel(string.Empty));
		}
	
		foreach (var capture in match.Groups["data"].Captures.Cast<Capture>())
		{	
			var entry = capture.Value == "|"
				? CreatePipe()
				: CreateSpace();
			
			panel.Children.Add(entry);
		}
		return panel;
	}
	
	private FrameworkElement CreatePipe()
	{
		return new Line(){Y2=unitHeight, Stroke=new SolidColorBrush(Colors.Gray), StrokeThickness=4, StrokeEndLineCap=PenLineCap.Round, Margin=new Thickness(unitMargin.Left + (unitWidth/2)-1,unitMargin.Top,unitMargin.Right+(unitWidth/2)-1,unitMargin.Bottom), VerticalAlignment=VerticalAlignment.Center};
	}
	private FrameworkElement CreateSpace()
	{
		return new Line(){X2=unitWidth, StrokeThickness=0, Margin=unitMargin, VerticalAlignment=VerticalAlignment.Center};
	}
}


public void SaveAsPng(StackPanel panel, int maxImageWidth, int maxImageHeight, string outputPath)
{
	var border = new Border();
	border.Padding = new Thickness(25);
	border.BorderThickness = new Thickness(3);
	border.CornerRadius = new CornerRadius(10);
	border.BorderBrush = new SolidColorBrush(Colors.Gray);
	border.Background = new SolidColorBrush(Colors.White);
	border.Child = panel;
	
	var viewbox = new Viewbox();
	viewbox.Stretch = Stretch.Uniform;
	viewbox.StretchDirection = StretchDirection.Both;
	viewbox.Child = border;
	viewbox.Arrange(new Rect(new Point(0, 0), new Point(maxImageWidth, maxImageHeight)));
	
	var bitmapFrame = CaptureVisual(viewbox);
    SaveToDisk(bitmapFrame, new PngBitmapEncoder(), outputPath);
}
private static BitmapFrame CaptureVisual(UIElement source)
{
	var actualHeight = source.RenderSize.Height;
	var actualWidth = source.RenderSize.Width;

	var renderTarget = new RenderTargetBitmap((int)actualWidth, (int)actualHeight, 96, 96, PixelFormats.Pbgra32);	
	var sourceBrush = new VisualBrush(source);
	var drawingVisual = new DrawingVisual();
	var drawingContext = drawingVisual.RenderOpen();
	
	using (drawingContext)
	{
		drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(actualWidth, actualHeight)));
	}
	renderTarget.Render(drawingVisual);
	return BitmapFrame.Create(renderTarget);
}

private static void SaveToDisk(BitmapFrame bitmapFrame, BitmapEncoder bitmapEncoder, string filename)
{
	bitmapEncoder.Frames.Add(bitmapFrame);
	using (var outputStream = File.OpenWrite(filename))
	{
		bitmapEncoder.Save(outputStream);
		outputStream.Flush();
	}
}