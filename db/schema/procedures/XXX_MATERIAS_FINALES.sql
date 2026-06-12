SET TERM ^ ;

ALTER PROCEDURE XXX_MATERIAS_FINALES (CODALU CHAR(11) CHARACTER SET NONE,
CARRE VARCHAR(6) CHARACTER SET NONE)
RETURNS (CODMAT CHAR(2) CHARACTER SET NONE,
MATERIA VARCHAR(60) CHARACTER SET NONE,
FERRCOD INTEGER,
FERRMSG VARCHAR(1000) CHARACTER SET NONE,
FERRWEB VARCHAR(1000) CHARACTER SET NONE,
CUTUCO INTEGER,
CONDICION VARCHAR(15) CHARACTER SET NONE,
FMESA INTEGER)
AS 
declare variable CORRELA varchar(100);
declare variable CONT integer;
declare variable CUAANIO integer;
declare variable SIGO char(1);
declare variable BANDERA char(1);
declare variable CODMATVER char(2);
declare variable MATCORRELA varchar(2000);
declare variable PASADO char(1);
declare variable TIPO varchar(3);
declare variable CA char(1);
declare variable DNI char(1);
declare variable CTT char(1);
declare variable FECH_CTT date;
begin
   --exception e_custom_err 'pepe';
   FERRCOD=0;
   FERRMSG='';
   MATCORRELA='';
   SELECT TIPO FROM CARRERA WHERE CARRE=:CARRE INTO :TIPO;

   SELECT A.ca, a.dni, a.fech_ctt, a.ctt
   FROM alumnos A
   WHERE CARRE=:carre AND A.cod_alu=:codalu
   INTO :ca, :dni, :fech_ctt, :ctt;

   IF (TIPO = 'TER') THEN
    BEGIN
      FOR SELECT C.COD_MAT, M.DESCRIPCI, M.CORREFINAL, C.CUTUCO, C.CUA_ANIO, TRIM(C.CONDICION),
                 CASE WHEN C.FINAL1 BETWEEN 1 AND 3.99 AND
                           C.FINAL2 BETWEEN 1 AND 3.99 AND
                           C.FINAL3 BETWEEN 1 AND 3.99 THEN '1' ELSE '0' END
      FROM CURSADA C
      LEFT OUTER JOIN MATERIAS M ON C.COD_MAT=M.CODMATERI AND C.CARRE=M.CODCARRE
      WHERE C.COD_ALU=:CODALU AND C.CARRE=:CARRE
      INTO :CODMAT, :MATERIA, :CORRELA, :CUTUCO, :CUAANIO, :CONDICION, :PASADO
      DO BEGIN
         FERRCOD=0; FERRMSG=''; MATCORRELA='';
         if (TRIM(CONDICION) <> 'REGULAR') then begin
            FERRCOD=2;
            FERRMSG=' La meteria debe estar en dodicion de regular';
            FERRWEB='No te podes inscribir a esta meteria porque no esta regular';
            SUSPEND;
         end
         else if (CTT = '*' and (fech_ctt is null or fech_ctt+90 <= current_date )) then begin
            FERRCOD=2;
            FERRMSG=' La CTT esta vencida';
            FERRWEB='No te podes inscribir a ninguna materia porque la Contancia de Titulo en Tramite se encuentra vencida';
            SUSPEND;
         end
         else if ((ca is null or ca <> '*') and (CTT = '*' and (fech_ctt is null or fech_ctt+90 <= current_date ))) then begin
             FERRCOD=2;
             FERRMSG=' Falta presentar el analitico';
             FERRWEB='No te podes inscribir a ninguna materia porque no esta presentado el Certificado Analitico';
             SUSPEND;
         end
         else if (dni is null or dni = '3') then begin
             FERRCOD=2;
             FERRMSG=' Falta presentar la fotocopia del DNI';
             FERRWEB='No te podes inscribir a ninguna materia porque no esta presentado la fotocopia del DNI';
             SUSPEND;
         end
         else IF (PASADO='1') THEN BEGIN
            FERRCOD=2;
            FERRMSG=' Paso el limite de finales';
            FERRWEB='Pasaste el limite de 3 finales rendidos. Tienes que recursar la metaria';
            SUSPEND;
         END ELSE IF (EXISTS(SELECT 1 FROM PERMEXA P WHERE P.CARRE=:CARRE AND P.COD_ALU=:CODALU AND P.COD_MAT=:CODMAT)) THEN BEGIN
            FERRCOD=2;
            FERRMSG=' Ya existe un permiso cargado para esta materia. Borrelo primero.';
            FERRWEB='Hay un permiso cargado para esta meteria. Habla con secretaria docente si no deberia de estar';
            SELECT P.mesa FROM PERMEXA P WHERE P.CARRE=:CARRE AND P.COD_ALU=:CODALU AND P.COD_MAT=:CODMAT into FMESA;
            SUSPEND;
         END
         ELSE IF ((SELECT LLAMADOS FROM XXX_CUENTA_LLAMADOS(:CUAANIO))>=9) THEN BEGIN
           FERRCOD=2;
           FERRMSG=' Al alumno se le vencio los llamados permitidos';
           FERRWEB='Se paso el limite de llamados para poder rendir y aprobar la materia';
           SUSPEND;
         END
         else IF (EXISTS(SELECT 1 FROM PERMEXA P WHERE P.CARRE=:CARRE AND P.COD_ALU=:CODALU AND P.COD_MAT=:CODMAT)) THEN BEGIN
            FERRCOD=2;
            FERRMSG=' Ya existe un permiso cargado para esta materia. Borrelo primero.';
            FERRWEB='Hay un permiso cargado para esta meteria. Habla con secretaria docente si no deberia de estar';
            SELECT P.mesa FROM PERMEXA P WHERE P.CARRE=:CARRE AND P.COD_ALU=:CODALU AND P.COD_MAT=:CODMAT into FMESA;
            SUSPEND;
         END
         ELSE IF (CORRELA IS NULL OR CORRELA='' OR CORRELA  CONTAINING '----') THEN BEGIN
             SUSPEND;
         END ELSE BEGIN
            CONT=1; MATCORRELA=''; FERRCOD=0;
            WHILE (CONT<=CHAR_LENGTH(TRIM(CORRELA))) DO BEGIN
                CODMATVER=SUBSTRING(CORRELA FROM CONT FOR 2);
                IF (CODMATVER IS NOT NULL AND CODMATVER<>'') THEN BEGIN
                   IF (NOT EXISTS(SELECT 1 FROM ANALITIC A WHERE A.COD_ALU=:CODALU AND A.CARRE=:CARRE AND A.COD_MAT=:CODMATVER)) THEN  BEGIN
                       SELECT :MATCORRELA||M.DESCRIPCI||ASCII_CHAR(13) FROM MATERIAS M WHERE M.CODCARRE=:carre AND M.codmateri=:codmatver INTO :MATCORRELA;
                   END
                END
                CONT=CONT+3;
            END
            --ESTE ALUMNO QUITARLO PORQUE ME LO PIDIO SANDRA
            --PARA NO CARGAR LOS FINALES DE LAS CORRELATIVAS
            --YA QUE ERA UN PASE 07/08/2013
            IF (MATCORRELA<>'') THEN BEGIN
              FERRCOD=2;
              FERRMSG=' El Alumno debe correlatividades a saber: '||ASCII_CHAR(13)||MATCORRELA;
              FERRWEB=' No podes anotarte a esta meteria ya que debes correlatividades: <br>'||replace(MATCORRELA,ASCII_CHAR(13),'<br>');
            END
            SUSPEND;
         END
      END
    END
  ELSE IF (TIPO IN ('BAC','BAD')) THEN
    BEGIN
      FOR SELECT C.COD_MAT, M.DESCRIPCI, M.CORRELATIV, C.CUTUCO, C.CONDICION
      FROM CURSADA C
      LEFT OUTER JOIN MATERIAS M ON C.COD_MAT=M.CODMATERI AND C.CARRE=M.CODCARRE
      WHERE C.COD_ALU=:CODALU AND C.CARRE=:CARRE AND TRIM(C.CONDICION) IN ('PREVIA','LIBRES','PREVIO','P/EQUIVALEN')
      INTO :CODMAT, :MATERIA, :CORRELA, :CUTUCO, :CONDICION
      DO BEGIN
         FERRCOD=0; FERRMSG='';
         IF (EXISTS(SELECT 1 FROM PERMEXA P WHERE P.CARRE=:CARRE AND P.COD_ALU=:CODALU AND P.COD_MAT=:CODMAT)) THEN BEGIN
            FERRCOD=2;
            FERRMSG=' Ya existe un permiso cargado para esta materia. Borrelo primero.';
            FERRWEB='Hay un permiso cargado para esta meteria. Habla con secretaria docente si no deberia de estar';
            SELECT P.mesa
            FROM PERMEXA P
            WHERE P.CARRE=:CARRE AND P.COD_ALU=:CODALU AND P.COD_MAT=:CODMAT
            order by p.indice desc
            rows 1
            into FMESA;
            SUSPEND;
         END
         ELSE
           SUSPEND;
      END
    END
end ^

SET TERM ; ^
