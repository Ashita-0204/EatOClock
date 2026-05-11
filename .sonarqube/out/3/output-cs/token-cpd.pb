Ã
6C:\Users\ashit\Desktop\Demo\EatOClock\Shared\Class1.cs
	namespace 	
Shared
 
; 
public 
class 
Class1 
{ 
} ”
CC:\Users\ashit\Desktop\Demo\EatOClock\Shared\Constants\ApiRoutes.cs
	namespace 	
Shared
 
. 
	Constants 
; 
public 
static 
class 
	ApiRoutes 
{ 
public 
const 
string 
AuthBase !
=" #
$str$ 1
;1 2
public 

const 
string 
Register  
=! "
$str# -
;- .
public 

const 
string 
Login 
= 
$str  '
;' (
public 

const 
string 
Refresh 
=  !
$str" +
;+ ,
public 

const 
string 
Profile 
=  !
$str" +
;+ ,
public		 

const		 
string		 
ChangePassword		 &
=		' (
$str		) :
;		: ;
}

 ‰	
<C:\Users\ashit\Desktop\Demo\EatOClock\Shared\DTOs\UserDTO.cs
	namespace 	
Shared
 
. 
Dtos 
; 
public 
class 
UserDTO 
{ 
public 

string 
Id 
{ 
get 
; 
set 
;  
}! "
public 

string 
Email 
{ 
get 
; 
set "
;" #
}$ %
public 

string 
FullName 
{ 
get  
;  !
set" %
;% &
}' (
public 
string 
Role 
{ 
get 
; 
set "
;" #
}$ %
public		 

string		 
ProfilePicUrl		 
{		  !
get		" %
;		% &
set		' *
;		* +
}		, -
public

 

bool

 
IsActive

 
{

 
get

 
;

 
set

  #
;

# $
}

% &
} ÿ
?C:\Users\ashit\Desktop\Demo\EatOClock\Shared\Enums\RoleEnums.cs
	namespace 	
Shared
 
. 
Enums 
; 
public 
enum 
RoleEnum 
{ 
Customer 
= 
$num 
, 
RestaurantOwner 
= 
$num 
, 
DeliveryAgent 
= 
$num 
, 
Admin 
= 
$num 
}		 