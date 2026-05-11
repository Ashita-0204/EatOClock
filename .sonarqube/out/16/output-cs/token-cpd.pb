ƒ
ZC:\Users\ashit\Desktop\Demo\EatOClock\BuildingBlocks\Authentication\JwtBearerExtensions.cs
	namespace 	
Authentication
 
; 
public 
static 
class 
JwtBearerExtensions '
{		 
public

 

static

 
IServiceCollection

 $&
AddCustomJwtAuthentication

% ?
(

? @
this

@ D
IServiceCollection

E W
services

X `
,

` a
IConfiguration

b p
config

q w
)

w x
{ 
var 
jwtKey 
= 
config 
[ 
$str %
]% &
;& '
var 
issuer 
= 
config 
[ 
$str (
]( )
;) *
var 
audience 
= 
config 
[ 
$str ,
], -
;- .
var 
key 
= 
Encoding 
. 
ASCII  
.  !
GetBytes! )
() *
jwtKey* 0
)0 1
;1 2
services 
. 
AddAuthentication "
(" #
options# *
=>+ -
{ 	
options 
. %
DefaultAuthenticateScheme -
=. /
JwtBearerDefaults0 A
.A B 
AuthenticationSchemeB V
;V W
options 
. "
DefaultChallengeScheme *
=+ ,
JwtBearerDefaults- >
.> ? 
AuthenticationScheme? S
;S T
} 	
)	 

. 	
AddJwtBearer	 
( 
options 
=>  
{ 	
options 
.  
RequireHttpsMetadata (
=) *
false+ 0
;0 1
options 
. 
	SaveToken 
= 
true  $
;$ %
options 
. %
TokenValidationParameters -
=. /
new0 3%
TokenValidationParameters4 M
{ $
ValidateIssuerSigningKey (
=) *
true+ /
,/ 0
IssuerSigningKey  
=! "
new# & 
SymmetricSecurityKey' ;
(; <
key< ?
)? @
,@ A
ValidateIssuer 
=  
true! %
,% &
ValidIssuer 
= 
issuer $
,$ %
ValidateAudience    
=  ! "
true  # '
,  ' (
ValidAudience!! 
=!! 
audience!!  (
,!!( )
ValidateLifetime""  
=""! "
true""# '
,""' (
	ClockSkew## 
=## 
TimeSpan## $
.##$ %
Zero##% )
}$$ 
;$$ 
}%% 	
)%%	 

;%%
 
return&& 
services&& 
;&& 
}'' 
}(( 