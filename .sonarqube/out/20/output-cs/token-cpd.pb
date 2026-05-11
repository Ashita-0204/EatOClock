°
KC:\Users\ashit\Desktop\Demo\EatOClock\BuildingBlocks\Caching\IRedisCache.cs
	namespace 	
Caching
 
; 
public 
	interface 
IRedisCache 
{ 
Task 
< 	
string	 
> 
GetStringAsync 
(  
string  &
key' *
)* +
;+ ,
Task 
SetStringAsync	 
( 
string 
key "
," #
string$ *
value+ 0
,0 1
TimeSpan2 :
?: ;
expiry< B
=C D
nullE I
)I J
;J K
Task		 
RemoveAsync			 
(		 
string		 
key		 
)		  
;		  !
}

 