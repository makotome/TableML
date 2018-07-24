﻿
namespace TableML.Compiler
{

    /// <summary>
    /// Default template, for Unity + KEngine
    /// </summary>
    public class LuaDefaultTemplate
    {
        public static string FileTableFileRow = @"-- This file is auto generated by SettingModuleEditor.cs!
-- Don't manipulate me!
-- Default Template for KEngine!
local TableFileRow = {}

-- create a ui instance
function TableFileRow.New(rowNumber,headerInfos)
    local newSelf = new(TableFileRow)
    newSelf.RowNumber = 0
    newSelf.IsAutoParse = false
    newSelf.HeaderInfos = {}
    newSelf.Values = {}
    if rowNumber ~= nil and headerInfos ~= nil then
        newSelf:ctor(rowNumber,headerInfos)
    end
    return newSelf
end

function TableFileRow:ctor( rowNumber,headerInfos )
    self.RowNumber = rowNumber
    for k,v in pairs(headerInfos) do
        self.HeaderInfos[k] = v
    end
end

function TableFileRow:setValues( value )
    for i=1,#value do
        self.Values[i]=value[i]
    end
end

function TableFileRow:parse( headers,cellStrs )
    local index = 1
    for k,v in pairs(headers) do
        self[k] = cellStrs[index]
        index = index + 1
    end
end


function TableFileRow:PrimaryKey()
    return self:GetPrimaryKey()
end

function TableFileRow:GetPrimaryKey()
    return self:GetByIndex(1);
        end

        function TableFileRow:GetByIndex(index )
    if index > #self.Values or index < 1 then
        print(string.format(""Overflow index %d"", index))
    end
    return self.Values[index]
end

function TableFileRow:SetByIndex(index , value)

    self.Values[index] = value
end

function TableFileRow:GetByString(headerName)
    local HeaderInfo = HeaderInfos[headerName]
    if HeaderInfo == nil then
        print(""not found header: ""..headerName)
    end
    return HeaderInfo
end

function TableFileRow:SetByString(headerName, value)
    HeaderInfos[headerName] = value
end

return TableFileRow";

        public static string FileTableFile = @"-- This file is auto generated by SettingModuleEditor.cs!
-- Don't manipulate me!
-- Default Template for KEngine!
function string:newSplit(sep)
    local sep, fields = sep or "":"", {}
    local pattern = string.format(""([^%s]+)"", sep)
    self:gsub(pattern, function (c) fields[#fields + 1] = c end)
    return fields
end

local TableFileRow = import(""Setting/TableFileRow"")
local TableFile = { }

function TableFile.New()
    local newSelf = new(TableFile)
    newSelf.Separator = '\t'
    newSelf.ReloadCount = 0
    newSelf.Headers={}
    newSelf._colCount=0
    newSelf.TabInfo={}
    newSelf._rowIndex = 1
    newSelf.PrimaryKey2Row={}
    newSelf.Rows = {}
    return newSelf
end

function TableFile:LoadFromFile(file)
    self.fileFullPath = CS.KEngine.KResourceModule.GetResourceFullPath(file,false,false)
    if self.fileFullPath == nil then
        self.fileFullPath = file
    end
    print(""TableFile:LoadFromFile : ""..self.fileFullPath)
    self:parseReader(self.fileFullPath)
    return self
end

function TableFile:parseReader(path)
    -- 首行
    local file = io.open(path, ""r"")
    local headLine = file:read(""*l"")
    if headLine == nil then
        print(""have not head line!"")
        return false
    end

    -- 声明行
    local metaLine = file:read(""*l"")
    if metaLine == nil then
        print(""have not meta line!"")
        return false
    end

    -- don't remove RemoveEmptyEntries!
    local firstLineSplitString = headLine:newSplit(self.Separator)
    local firstLineDef = { }
    local metaLineArr = metaLine:newSplit(self.Separator)

    -- 拷贝，确保不会超出表头的
    for i=1,#metaLineArr do
        firstLineDef[i] = metaLineArr[i]
    end

    for i=1,#firstLineSplitString do
        local headerString = firstLineSplitString[i]
        local headerInfo = {
            ColumnIndex=i,
            HeaderName=headerString,
            HeaderMeta=firstLineDef[i],
        }
        self.Headers[headerInfo.HeaderName] = headerInfo
    end

    -- 標題
    self._colCount = #firstLineSplitString

    -- 读取行内容
    local sLine = """"
    while sLine ~= nil do
        local sLine = file:read(""*l"")
        if sLine ~= nil then
            local splitString1 = sLine:newSplit(self.Separator)
            self.TabInfo[self._rowIndex] = splitString1

            local newT = TableFileRow.New()
            newT:ctor(self._rowIndex, self.Headers)
            newT:setValues(splitString1)
            newT:parse(self.Headers, splitString1)
            self.PrimaryKey2Row[newT:GetPrimaryKey()] = newT
            self.Rows[self._rowIndex] = newT;
            self._rowIndex = self._rowIndex + 1
        else
            break
        end
    end
    io.close(file)
end

function TableFile:getRowCount()
    return #self.Rows
end

function TableFile:getColumnCount()
    return self._colCount;
end

function TableFile:hasPrimaryKey(primaryKey)
    if self.PrimaryKey2Row[primaryKey] ~= nil then
        return true
    else
        return false

    end
end

function TableFile:getByPrimaryKey(primaryKey )
    return self.PrimaryKey2Row[primaryKey]
end

function TableFile:getAll()
    return self.Rows
end

return TableFile";
        
        public static string LuaGenCodeTemplate = @"-- This file is auto generated by SettingModuleEditor.cs!
-- Don't manipulate me!
-- Default Template for KEngine!

function new(table, ctorFunc)
    assert(table ~= nil)

    table.__index = table

    local tb = {}
    setmetatable(tb, table)

    if ctorFunc then
        ctorFunc(tb)
    end

    return tb
end

local TableFile = import(""Setting/TableFile"")
{% for file in Files %}
local {{file.ClassName}}Setting = { }
local {{file.ClassName}}Settings = { }

function {{file.ClassName}}Settings.New()
    local newSelf = new({{file.ClassName}}Settings)
    newSelf.tabFilePath = {{ file.TabFilePaths }}
    newSelf.ReloadCount = 0
    newSelf._dict = {}
    return newSelf
end

function {{file.ClassName}}Settings:GetInstance()
    if self.ReloadCount == 0 then
        self:ReloadAll()
    end
    return self
end

function {{file.ClassName}}Settings:ReloadAll()
    local tableFile = TableFile.New()
    local path = nil

    if CS.KEngine.Log.IsUnityEditor ~= true then
        path = ""{{ BundlesPath }}""..""/""..self.tabFilePath
    else
        path = ""{{ CompilePath }}""..""/""..self.tabFilePath
    end

    print("" ==================== path : ""..path)

    tableFile:LoadFromFile(path)
    for i,row in ipairs(tableFile:getAll()) do
        local pk = {{file.ClassName}}Setting.ParsePrimaryKey(row)
        local setting = self._dict[pk]
        if setting == nil then
            setting = {{file.ClassName}}Setting.New(row)
            self._dict[setting.Id] = setting
        else
            setting:Reload(row)
        end
        self.ReloadCount = self.ReloadCount + 1
    end
end

function {{file.ClassName}}Settings:Get(primaryKey)
    return self._dict[primaryKey]
end

function {{file.ClassName}}Setting.New(row)
    local newSelf = new({{file.ClassName}}Setting)
    newSelf:Reload(row)
    return newSelf
end

function {{file.ClassName}}Setting:Reload(row){% for field in file.Fields %}
    self.{{ field.Name}} = row.Values[{{ field.Index }} + 1]{% endfor %}
end

function {{file.ClassName}}Setting.ParsePrimaryKey(row)
    local primaryKey = row.Values[1]
    return primaryKey
end
{% endfor %}

AppSettings={}
function AppSettings:init(){% for file in Files %}
    self.{{file.ClassName}}Settings = {{file.ClassName}}Settings.New(){% endfor %}
end
";
    }
}
