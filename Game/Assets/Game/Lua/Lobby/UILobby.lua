-- UILobby.lua
-- Author : Dzq
-- Date : 2017-02-22
-- Last modification : 2017-02-23
-- Desc: 大厅

module(...,package.seeall)

local item ={} 
item.Top = UnityEngine.GameObject
item.Center = UnityEngine.GameObject
item.Bottom = UnityEngine.GameObject
item.name = TextEx
item.gold = TextEx
item.icon = ImageEx
item.btnCreate = StateHandle
item.btnEnter = StateHandle
item.btnSetting = StateHandle

local itemDefine = util.Clone(item)

util.GetMonoTable(activeGroup:Get(1),itemDefine,item) --activeGroup 是UIGroup

function Init()	
	item.name.text = tostring("testName")
	item.icon.Set("sprinte1")
end

function Open()
	
end

function Fresh()
	
end

function Close()
	
end



function OnOpen()

end

function OnClose()
	
end

function OnUpdate()
	
end

function OnFresh()
	
end

function OnDestroy()

end