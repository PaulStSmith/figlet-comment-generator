program TestProgram;

type
  TestClass = class
  public
    procedure TestMethod;
  end;

procedure TestClass.TestMethod;
begin
  WriteLn('Hello World!');
end;

var
  obj: TestClass;
begin
  obj := TestClass.Create;
  try
    obj.TestMethod;
  finally
    obj.Free;
  end;
end.