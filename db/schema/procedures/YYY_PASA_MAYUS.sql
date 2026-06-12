SET TERM ^ ;

ALTER PROCEDURE YYY_PASA_MAYUS (TEXT VARCHAR(500) CHARACTER SET NONE)
RETURNS (FTEXTO VARCHAR(500) CHARACTER SET NONE)
AS 
declare variable X integer;
declare variable LETRA varchar(1);
begin
X=1;
FTEXTO='';
WHILE (X<=char_length(TEXT)) DO BEGIN
   LETRA=SUBSTRING(TEXT FROM X FOR 1);
   if (LETRA = 'á') then LETRA='Á';
   ELSE if (LETRA = 'é') then LETRA='É';
   ELSE if (LETRA = 'í') then LETRA='Í';
   ELSE if (LETRA = 'ó') then LETRA='Ó';
   ELSE if (LETRA = 'ú') then LETRA='Ú';
   ELSE if (LETRA = 'ñ') then LETRA='Ñ';
   ELSE LETRA=UPPER(LETRA);
   FTEXTO=FTEXTO||LETRA;
   X=X+1;
END
SUSPEND;
end ^

SET TERM ; ^
