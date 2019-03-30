<%
response.charset = "windows-1254"
DB_DRIVER	= "MySQL ODBC 8.0 UNICODE Driver" ' This declaretion for Mysql
DB_SERVER	= "" 'server name
DB_NAME		= "" 'database name
DB_UID	= "" 'username
DB_PWD	= "" 'user password
DB_CHARSET= "UTF8"
DB_OPTION	= "3"
DB_PORT	= "3306"

constr ="DRIVER={"& DB_DRIVER &"};SERVER="& DB_SERVER &";DB="& DB_NAME &";UID="& DB_UID &";PWD="& DB_PWD &";CHARSET="& DB_CHARSET &";OPTION="& DB_OPTION &";PORT="& DB_PORT &";"

Dim Connection, Recordset,Command
Set Connection = Server.CreateObject("ADODB.Connection")
Set Recordset = Server.CreateObject("ADODB.Recordset")
Set Command = Server.CreateObject("ADODB.Command")
Connection.ConnectionString=constr


' Get a Result set using by sql select query
public function GetData(sqlquery)

      If Connection.State = 0 Then 
        Connection.Open
      end If

    set GetData =Connection.Execute(sqlquery) 

end function 

'Add a New Record to Database using by table name and table column values
 public function AddData(tableName,values)

    dim cols,query
    set Recordset=GetData("SELECT COLUMN_NAME AS COL, DATA_TYPE AS DATYPE FROM information_schema.columns WHERE table_schema='"&DB_NAME&"' AND table_name='"&tableName&"' AND COLUMN_NAME!='Id'")

        While Not Recordset.EOF        
            cols = cols & Recordset.Fields("COL")&","
            vals = vals &"?"&","            
            Recordset.MoveNext
        Wend

    cols=Mid(cols,1,Len(cols)-1)
    vals=Mid(vals,1,Len(vals)-1)

    query="INSERT "& tableName &"(" &cols&") VALUES("&vals&")"

    resultmsg=ExecuteNonQuery(query,values)
    set Recordset= GetData("SELECT LAST_INSERT_ID();")
    AddData=Recordset.Fields(0)

 end function

'Set a Record  using by table name and table column values
public function SetData(tableName,values)

    dim cols,query
    set Recordset=GetData("SELECT COLUMN_NAME AS COL, DATA_TYPE AS DATYPE FROM information_schema.columns WHERE table_schema='"&DB_NAME&"' AND table_name='"&tableName&"' AND COLUMN_NAME!='Id'")

        While Not Recordset.EOF        
            cols = cols & Recordset.Fields("COL")&"="&"?"&","      
            Recordset.MoveNext
        Wend

    cols=Mid(cols,1,Len(cols)-1)
    query=" UPDATE "& tableName &" SET " &cols&" WHERE Id = ? "
    RecordsAffected=ExecuteNonQuery(query,values)

    If err.number > 0 or RecordsAffected = 0 then
         SetData ="İşlem başarısız oldu !"
        Else
         SetData ="İşlem gerçekleştirildi."

    end if
end function

'Delete a Record  using by recordId
public function DelData(tableName,recordId)

    query="Delete From "& tableName &" WHERE Id=?"
    Set params = CreateObject("System.Collections.ArrayList")
    params.add recordId
    RecordsAffected=ExecuteNonQuery(query,params)

    If err.number > 0 or RecordsAffected = 0 then
         DelData ="İşlem başarısız oldu !"
        Else
         DelData ="İşlem gerçekleştirildi."

    end if
end function

private function ExecuteNonQuery(query,params)
      If Connection.State = 0 Then 
        Connection.Open
      end If
    Command.ActiveConnection=Connection
    Command.CommandType=1
    Command.CommandText=query

    counter=0
    While counter<params.Count
       Command.Parameters(counter)=params(counter)
        counter=counter+1
    Wend
    counter=0

    Command.Execute RecordsAffected, , adExecuteNoRecords
    ExecuteNonQuery=RecordsAffected
    Connection.Close()

end function


 %>