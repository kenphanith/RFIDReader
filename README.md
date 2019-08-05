# RFIDReader

### Usage
+ get dll and add to the project by following this guidelines: https://qiita.com/phanithken/items/fffa6ec4b73b1fdde254
+ sample code
```
private RFIDReader _rfidReader = new RFIDReader();
_rfidReader.OnDataTag += new RFIDDataHandler(this.GetTagData);
_rfidReader.Start();

private void GetTagData(object sender, RFIDEventArgs e)
{
    // tag data
    string tag = e;
}
```
