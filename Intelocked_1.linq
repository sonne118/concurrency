<Query Kind="Statements">
  <Output>DataGrids</Output>
</Query>

string[] myArray = new string[] { "A", "B", "C" };
string myStr = Interlocked.CompareExchange(ref myArray[0], "F", myArray[0]);
foreach (var item in myArray)
{
	Console.WriteLine(item.ToString());
}