@tool
class_name ToastItem
extends PanelContainer

var _label: Label
var _text: String = ""

@export var text: String:
	get:
		return _text
	set(value):
		_text = value
		# 确保节点已初始化，防止在 _ready 之前赋值报错
		if is_instance_valid(_label):
			_label.text = _text

func _ready() -> void:
	# 使用 % 获取场景唯一节点，确保即使节点改名或移动层级也能找到
	_label = get_node_or_null("%TextLabel3yb7r")
	
	# 同步一次初始文本
	if is_instance_valid(_label):
		_label.text = _text
