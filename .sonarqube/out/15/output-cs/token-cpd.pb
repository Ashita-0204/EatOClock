ª
KC:\Users\ashit\Desktop\Demo\EatOClock\BuildingBlocks\HealthChecks\Class1.cs
	namespace		 	
HealthChecks		
 
;		 
public 
static 
class !
HealthCheckExtensions )
{ 
public 

static  
IHealthChecksBuilder &"
AddServiceHealthChecks' =
(= >
this> B
IServiceCollectionC U
servicesV ^
)^ _
{ 
return 
services 
. 
AddHealthChecks '
(' (
)( )
. 
AddCheck 
( 
$str 
, 
( 
)  
=>! #
HealthCheckResult$ 5
.5 6
Healthy6 =
(= >
$str> R
)R S
,S T
tagsU Y
:Y Z
new[ ^
[^ _
]_ `
{a b
$strc i
}j k
)k l
;l m
} 
public 

static !
IEndpointRouteBuilder '"
MapServiceHealthChecks( >
(> ?
this? C!
IEndpointRouteBuilderD Y
	endpointsZ c
)c d
{ 
	endpoints 
. 
MapHealthChecks !
(! "
$str" +
,+ ,
new- 0
HealthCheckOptions1 C
{ 	
ResponseWriter 
= 
async "
(# $
context$ +
,+ ,
report- 3
)3 4
=>5 7
{ 
context 
. 
Response  
.  !
ContentType! ,
=- .
$str/ A
;A B
await   
context   
.   
Response   &
.  & '

WriteAsync  ' 1
(  1 2
JsonSerializer  2 @
.  @ A
	Serialize  A J
(  J K
new  K N
{!! 
status"" 
="" 
report"" #
.""# $
Status""$ *
.""* +
ToString""+ 3
(""3 4
)""4 5
,""5 6
	timestamp## 
=## 
DateTime##  (
.##( )
UtcNow##) /
}$$ 
)$$ 
)$$ 
;$$ 
}%% 
}&& 	
)&&	 

;&&
 
return'' 
	endpoints'' 
;'' 
}(( 
})) 