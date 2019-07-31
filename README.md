# RFIDReader

### Usage
+ get dll and add to the project by following this guidelines: https://qiita.com/phanithken/items/fffa6ec4b73b1fdde254
+ sample code
```
private RFIDReader _rfidReader = new RFIDReader();
_rfidReader.OnDataTag += GetTagData;

private void GetTagData(object sender, RFIDEventArgs e)
{
    // tag data
    string tag = e;
}
```
