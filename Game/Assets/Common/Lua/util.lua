--------------------------------------------------------------------------------
--      一些工具函数函数
--------------------------------------------------------------------------------

module(...,package.seeall)

local compType = typeof(UnityEngine.Component)
local goType = typeof(UnityEngine.GameObject)
local monoTableType = typeof(UI.MonoTable)
-- 传入一个游戏对象和类型定义，返回table，如果table没有传进来则新建一个
function GetMonoTable(gameObject,define,table)
  table = table or {}
  local monoTable =gameObject:GetComponent(monoTableType)
  
  for k,v in pairs(define) do
    --获取类型
    local valueType
    if v == nil or type(v)~="table" then
      valueType = nil
    else
      valueType =  typeof(v)        
      if(valueType == nil) then 
        valueType = nil 
      elseif  goType:Equals(valueType) then --游戏对象
        valueType = true
      elseif valueType:IsSubclassOf(compType) ==false then 
        valueType = nil
      end
    end
        
    -- 设置值到表上
    if valueType == nil then 
      table[k] = v
    elseif  valueType  ==true then--游戏对象
      table[k] = monoTable:getv(k)
    else
      local go =monoTable:getv(k) 
      if go then
        table[k] = go:GetComponent(valueType)
      end
    end
  end
end

--浅复制表，如果to没有传进来，那么新建一个返回
function Clone(from,to)
  to = to or {}
  for k,v in pairs(from) do
      to[k] =v
  end
  return to
end

function CheckTableForPrint(table)
  local t2= {}
  for k,v in pairs(table) do
    local vType = type(v)
    if vType == "table" then
      t2[k] =CheckTableForPrint(v)
    elseif vType == "userdata" then
      t2[k] =tostring(v)
    else
      t2[k] =v
    end      
  end
  return t2
end

--返回表的字符串表示，cjson不能转换userdata，这里要转换下
function PrintTable(table,notPrint)
  local log = cjson.encode(CheckTableForPrint(table))
  if notPrint == nil or notPrint==false then
    LogError(log)
  end
  return log
end

function IsArray(table)
  if type(table) ~= "table" then return false end
  for k,_ in pairs(table)  do      
      if type(k) ~= "number" then
        return false
      end
  end  
  return true
end

--导表出来的lua数组格式有问题，这里转换下
function ParseCfgArray(old)
  local newArray = {}
  if old == nil or type(old) ~= "table" then return newArray end
  
  --如果是从0开始的数组，转成从1开始
  if old[0] ~= nil then 
    for i = 0,#old do 
      newArray[i+1] =old[i][1]
    end  
  --弄紧凑
  elseif IsArray(old) then
    local i = 1
    local kvs = {}
    for k,v in pairs(old)  do      
      kvs[#kvs+1]={k,v}
    end  
    table.sort(kvs,function(a,b) return a[1]<b[1] end)
    for i =1,#kvs do
      newArray[#newArray+1] =kvs[i][2]
    end
  --键值对转数组
  else
    local i = 1
    for k,v in pairs(old)  do      
      newArray[i] =v
      i =i+1
    end  
  end
  
  return newArray
end

-- 保证正确调用,无论是lua函数或者c#委托
function SafeCall(fun,...)
  if fun == nil then return end
  if type(fun) == "userdata" then
    return UI.UILuaPanel.TryCall(fun,...)
  else
    return fun(...)
  end
end
    
-- 根据值找到key
function IndexOf(table,findValue)
  for k,v in pairs(table) do
    if findValue == v then
      return k
    end
  end
end

--这里会加上堆栈，Debuger没有打印堆栈，print好像只有当前文件的堆栈
function LogError(log)
  Debugger.LogError(""..log.."\n"..debug.traceback())
end

--这里会加上堆栈，Debuger没有打印堆栈，print好像只有当前文件的堆栈
function Log(log)
  Debugger.Log(""..log.."\n"..debug.traceback())
end


