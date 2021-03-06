USE [master]
GO
/****** Object:  Database [NZULM]    Script Date: 24/11/2017 4:12:15 PM ******/
CREATE DATABASE [NZULM]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'NZULM', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\NZULM.mdf' , SIZE = 209920KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'NZULM_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\NZULM_log.ldf' , SIZE = 5696KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [NZULM] SET COMPATIBILITY_LEVEL = 100
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [NZULM].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [NZULM] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [NZULM] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [NZULM] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [NZULM] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [NZULM] SET ARITHABORT OFF 
GO
ALTER DATABASE [NZULM] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [NZULM] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [NZULM] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [NZULM] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [NZULM] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [NZULM] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [NZULM] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [NZULM] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [NZULM] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [NZULM] SET  DISABLE_BROKER 
GO
ALTER DATABASE [NZULM] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [NZULM] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [NZULM] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [NZULM] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [NZULM] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [NZULM] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [NZULM] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [NZULM] SET RECOVERY FULL 
GO
ALTER DATABASE [NZULM] SET  MULTI_USER 
GO
ALTER DATABASE [NZULM] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [NZULM] SET DB_CHAINING OFF 
GO
ALTER DATABASE [NZULM] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [NZULM] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [NZULM] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'NZULM', N'ON'
GO
ALTER DATABASE [NZULM] SET QUERY_STORE = OFF
GO
USE [NZULM]
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [NZULM]
GO
/****** Object:  UserDefinedFunction [dbo].[GetIngredientStrength]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Peter Jordan
-- Create date: January 13th 2012
-- Description:	Get Ingredient Strength description
-- =============================================
CREATE FUNCTION [dbo].[GetIngredientStrength]
(
	@PSTR varchar(5),
    @BaseFormStrengthNumeratorValue float,
    @BaseFormStrengthNumeratorUnits nvarchar(255),
    @BaseFormStrengthDenominatorValue float,
    @BaseFormStrengthDenominatorUnits nvarchar(255),
    @BaseFormStrengthOtherRepresentation nvarchar(255),
    @SaltFormStrengthNumeratorValue float,
    @SaltFormStrengthNumeratorUnits nvarchar(255),
    @SaltFormStrengthDenominatorValue float,
    @SaltFormStrengthDenominatorUnits varchar(255),
    @SaltFormStrengthOtherRepresentation nvarchar(255)
)
RETURNS varchar(255)
AS
BEGIN

	DECLARE @ReturnValue varchar(255)
	SET @ReturnValue = ''
 
    DECLARE @AltStrength varchar(255)
    SET @AltStrength = ISNULL(@SaltFormStrengthOtherRepresentation,@BaseFormStrengthOtherRepresentation)

    DECLARE @BaseNumerator varchar(255)
    SET @BaseNumerator = RTRIM(CAST(@BaseFormStrengthNumeratorValue as Varchar)) + ' ' + @BaseFormStrengthNumeratorUnits
    
    DECLARE @BaseDenominator varchar(255)
    SET @BaseDenominator = RTRIM(CAST(@BaseFormStrengthDenominatorValue as Varchar)) + ' ' + @BaseFormStrengthDenominatorUnits
    
    DECLARE @SaltNumerator varchar(255)
    SET @SaltNumerator = RTRIM(CAST(@SaltFormStrengthNumeratorValue as Varchar)) + ' ' + @SaltFormStrengthNumeratorUnits
    
    DECLARE @SaltDenominator varchar(255)
    SET @SaltDenominator = RTRIM(CAST(@SaltFormStrengthDenominatorValue as Varchar)) + ' ' + @SaltFormStrengthDenominatorUnits
     
    DECLARE @Strength varchar(255)
    
    if ISNULL(@SaltFormStrengthNumeratorValue,0) <> 0 BEGIN
		SET @strength = @SaltNumerator
		if ISNULL(@SaltFormStrengthDenominatorValue,0) <> 0 BEGIN
			SET @strength = @SaltNumerator + ' / ' + @SaltDenominator
		END
    END ELSE BEGIN
		SET @strength = @BaseNumerator
		if ISNULL(@BaseFormStrengthDenominatorValue,0) <> 0 BEGIN
			SET @strength = @BaseNumerator + ' / ' + @BaseDenominator
		END
    END  
    
	--No Strength Representation
    if @PSTR = 'NSR' BEGIN
		SET @ReturnValue = ''
	END
	
	--alternate strength only
	if @PSTR = 'AO' BEGIN
		SET @ReturnValue = @AltStrength
	END
	
	--alternate strength followed by numerator/denominator strength
	if @PSTR = 'AND' BEGIN
		SET @ReturnValue = @AltStrength + ' (' + @strength + ')'
	END
	
	--numerator/denominator strength
	if @PSTR = 'ND' BEGIN
		SET @ReturnValue = @strength
	END
	
	--numerator/denominator strength followed by alternate strength
	if @PSTR = 'NDA' BEGIN
		SET @ReturnValue = @strength +  ' (' + @AltStrength + ')'
	END
	
    --SET @ReturnValue = @PSTR
    				
	RETURN @ReturnValue

END

GO
/****** Object:  Table [dbo].[Prescribing_Term_Selection_List]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Prescribing_Term_Selection_List](
	[concept_id] [varchar](19) NOT NULL,
	[term_type] [varchar](10) NULL,
	[description_id] [varchar](19) NULL,
	[prescribing_term] [nvarchar](255) NULL,
 CONSTRAINT [PK_Prescribing_Term_Selection_List] PRIMARY KEY CLUSTERED 
(
	[concept_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ATC_Links]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ATC_Links](
	[ATC_LinksID] [bigint] NOT NULL,
	[SCTID] [varchar](19) NOT NULL,
	[ATC_Code] [varchar](10) NOT NULL,
	[Table_Name] [varchar](30) NOT NULL,
	[Is_Primary] [bit] NOT NULL,
 CONSTRAINT [PK_ATC_Links] PRIMARY KEY CLUSTERED 
(
	[ATC_LinksID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[ATC_Codes]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ATC_Codes](
	[ATC_CodesID] [bigint] NOT NULL,
	[ATC_Code] [varchar](10) NULL,
	[ATC_Name] [nvarchar](255) NULL,
 CONSTRAINT [PK_ATC_Codes] PRIMARY KEY CLUSTERED 
(
	[ATC_CodesID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetPrescribingTerm_ByType]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Author:		Peter Jordan
-- Create date: June 12, 2012
-- Description:	Prescribing Terms

CREATE FUNCTION [dbo].[GetPrescribingTerm_ByType] 
(	
	@Term varchar(max),
	@TermType varchar(10)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
		PTSL.concept_id,
		PTSL.prescribing_term,
		PTSL.term_type,
		ATCC.ATC_Name
	FROM [Prescribing_Term_Selection_List] PTSL
	LEFT OUTER JOIN [ATC_Links] ATCL ON ATCL.SCTID = PTSL.concept_id
	LEFT OUTER JOIN [ATC_Codes] ATCC ON ATCC.ATC_CODE = ATCL.ATC_CODE
	WHERE PTSL.prescribing_term LIKE '%' + @Term + '%'
	AND (PTSL.term_type = @TermType OR @TermType = 'both')
	
	--MPUU.MedicinalProductUnitOfUseID as [MPUU],
	--MP.PreferredTerm as [Product],
	----DSC.Term as [Product Synonym],
	----DT.[type_name] as [Desc Type],
	--MPUU.PreferredTerm as [Unit of Use],
	--MPP.PreferredTerm as [Pack]
	--FROM	[MedicinalProductUnitOfUse] MPUU 
	--LEFT OUTER JOIN [MedicinalProduct] MP ON MP.MedicinalProductID = MPUU.MedicinalProductID
	--LEFT OUTER JOIN [MedicinalProductPack_UnitOfUse] MHM ON MHM.MedicinalProductUnitOfUseID = MPUU.MedicinalProductUnitOfUseID
	--LEFT OUTER JOIN [MedicinalProductPack] MPP ON MPP.MedicinalProductPackID = MHM.MedicinalProductPackID
	----LEFT OUTER JOIN [Synonyms] SYN ON SYN.primary_sctID = MP.MedicinalProductID
	----LEFT OUTER JOIN [Descriptions] DSC ON DSC.parent_ID = SYN.synonym_sctID
	----LEFT OUTER JOIN [DescriptionType] DT ON DT.DescriptionTypeID = DSC.description_type_id
	--WHERE MPUU.preferredterm LIKE '%' + @Term + '%'
	--AND MPUU.status = 0
	----AND ISNULL(DT.[short_code],'X') = 'PT'
	----AND ISNULL(DSC.Term,'') <> MP.PreferredTerm
)



GO
/****** Object:  Table [dbo].[Prescribing_Term_Index]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Prescribing_Term_Index](
	[ctpp_id] [varchar](19) NOT NULL,
	[ctpp_pt] [nvarchar](1000) NULL,
	[ctpp_pharmacode] [int] NULL,
	[ctpp_atc_code] [varchar](15) NULL,
	[generic_prescribing_term] [nvarchar](255) NULL,
	[generic_concept_id] [varchar](19) NULL,
	[generic_concept_table] [varchar](30) NULL,
	[generic_description_id] [varchar](19) NULL,
	[trade_prescribing_term] [nvarchar](255) NULL,
	[trade_concept_id] [varchar](19) NULL,
	[trade_concept_table] [varchar](30) NULL,
	[trade_description_id] [varchar](19) NULL,
	[prescribe_by_brand] [bit] NOT NULL,
	[is_subsidised] [bit] NOT NULL,
 CONSTRAINT [PK_Prescribing_Term_Index] PRIMARY KEY CLUSTERED 
(
	[ctpp_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetMpuuId_ByTpuuId]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Author:		Peter Jordan
-- Create date: June 13, 2012
-- Description:	Translate Trade Concept ID to Generic MPUU Concept ID

CREATE FUNCTION [dbo].[GetMpuuId_ByTpuuId] 
(	
	@TradeConceptID varchar(19)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT PTI.generic_concept_id
	FROM [Prescribing_Term_Index] PTI
	WHERE PTI.trade_concept_id = @TradeConceptID
	AND PTI.trade_concept_table = 'TPUU'
	AND PTI.generic_concept_table = 'MPUU'
	
)


GO
/****** Object:  Table [dbo].[Substance]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Substance](
	[SubstanceID] [varchar](19) NOT NULL,
	[FullySpecifiedName] [nvarchar](255) NULL,
	[PreferredTerm] [nvarchar](255) NULL,
	[HasLessModifiedIngredientID] [varchar](19) NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_Ingredient] PRIMARY KEY CLUSTERED 
(
	[SubstanceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MedicinalProductUnitOfUse_SpecialActiveIngredient]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicinalProductUnitOfUse_SpecialActiveIngredient](
	[MedicinalProductUnitOfUse_SpecialActiveIngredientID] [varchar](19) NOT NULL,
	[MedicinalProductUnitOfUseID] [varchar](19) NULL,
	[Base_SubstanceID] [varchar](19) NULL,
	[Salt_SubstanceID] [varchar](19) NULL,
	[PreferredStrengthRepresentationTypeID] [varchar](19) NULL,
	[PreferredTermOrder] [float] NULL,
	[BaseFormStrengthNumeratorValue] [float] NULL,
	[BaseFormStrengthNumeratorUnitsID] [varchar](19) NULL,
	[BaseFormStrengthDenominatorValue] [float] NULL,
	[BaseFormStrengthDenominatorUnitsID] [varchar](19) NULL,
	[BaseFormStrengthOtherRepresentation] [nvarchar](255) NULL,
	[SaltFormStrengthNumeratorValue] [float] NULL,
	[SaltFormStrengthNumeratorUnitsID] [varchar](19) NULL,
	[SaltFormStrengthDenominatorValue] [float] NULL,
	[SaltFormStrengthDenominatorUnitsID] [varchar](19) NULL,
	[SaltFormStrengthOtherRepresentation] [nvarchar](255) NULL,
	[BossType] [char](4) NULL,
 CONSTRAINT [PK_MedicinalProductUnitOfUse_SpecialActiveIngredient] PRIMARY KEY CLUSTERED 
(
	[MedicinalProductUnitOfUse_SpecialActiveIngredientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetIngredients_Medicines_ByIngredientTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Author:		Peter Jordan
-- Create date: June 13, 2012
-- Description:	Get Medicines containing passed ingredient

CREATE FUNCTION [dbo].[GetIngredients_Medicines_ByIngredientTerm] 
(	
	@Term nvarchar(255)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
			ING.[PreferredTerm] as [Substance],
			ISNULL(NULLIF(PTSL.term_type,''),PTSL2.term_type) as [Term_Type],
			ISNULL(NULLIF(PTSL.prescribing_term,''),PTSL2.prescribing_term) as [Prescribing_Term]
		  FROM [Substance] ING
		  LEFT OUTER JOIN [MedicinalProductUnitOfUse_SpecialActiveIngredient] MPUUSAI ON 
				ING.SubstanceID = ISNULL(NULLIF(MPUUSAI.Base_SubstanceID,''),MPUUSAI.Salt_SubstanceID)
		  INNER JOIN [Prescribing_Term_Index] PTI ON PTI.generic_concept_id = MPUUSAI.MedicinalProductUnitOfUseID
		  LEFT OUTER JOIN [Prescribing_Term_Selection_List] PTSL ON LTRIM(PTSL.concept_id) = PTI.generic_concept_id
		  LEFT OUTER JOIN [Prescribing_Term_Selection_List] PTSL2 ON LTRIM(PTSL2.concept_id) = PTI.trade_concept_id
		  WHERE ING.PreferredTerm like '%' + @Term + '%'
)




GO
/****** Object:  UserDefinedFunction [dbo].[GetNZULM_PrescribingTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- Author:		Peter Jordan
-- Create date: August 19, 2016
-- Description:	Prescribing Terms

CREATE FUNCTION [dbo].[GetNZULM_PrescribingTerm] 
(	
	@Term varchar(max),
	@TermType varchar(10)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
		concept_id,
		prescribing_term,
		term_type
	FROM [Prescribing_Term_Selection_List]	
	WHERE prescribing_term LIKE '%' + @Term + '%'
	AND (term_type = @TermType OR @TermType = 'all')
	)
	


GO
/****** Object:  Table [dbo].[MedicinalProduct]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicinalProduct](
	[MedicinalProductID] [varchar](19) NOT NULL,
	[PreferredTerm] [nvarchar](1000) NULL,
	[FullySpecifiedname] [nvarchar](1000) NULL,
	[IsBaseSubstance] [bit] NOT NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_MedicinalProduct] PRIMARY KEY CLUSTERED 
(
	[MedicinalProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[MedicinalProduct_Has_Substance]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicinalProduct_Has_Substance](
	[MedicinalProduct_Has_SubstanceID] [varchar](19) NOT NULL,
	[MedicinalProductID] [varchar](19) NOT NULL,
	[SubstanceID] [varchar](19) NOT NULL,
 CONSTRAINT [PK_MedicinalProduct_Has_Substance] PRIMARY KEY CLUSTERED 
(
	[MedicinalProduct_Has_SubstanceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetMedicinalProduct_Substance_ByProductCode]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- Author:		Peter Jordan
-- Create date: 11, October 2016
-- Description:	Returns Medicinal Product and Ingredients for passed Code

CREATE FUNCTION [dbo].[GetMedicinalProduct_Substance_ByProductCode] 
(	
	@Code varchar(19)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
	MP.MedicinalProductID as [Product Code],
	MP.PreferredTerm as [PreferredTerm],
	MP.FullySpecifiedname as [FullySpecifiedName],
	MP.IsBaseSubstance as [BaseSubstance?],
	ISNULL(SUB.PreferredTerm,'') as [Ingredient],
	ISNULL(SUB2.PreferredTerm,'') as [LessModifiedIngredient]
	FROM [MedicinalProduct] MP
	LEFT OUTER JOIN MedicinalProduct_Has_Substance MPHS ON MPHS.MedicinalProductID = MP.MedicinalProductID
	LEFT OUTER JOIN Substance SUB ON SUB.SubstanceID = MPHS.SubstanceID
	LEFT OUTER JOIN Substance SUB2 ON SUB2.SubstanceID = SUB.HasLessModifiedIngredientID
	WHERE MP.MedicinalProductID = @Code
	AND MP.is_retired = 0
	AND ISNULL(SUB.is_retired,0) = 0
	AND ISNULL(SUB2.is_retired,0) = 0
)



GO
/****** Object:  Table [dbo].[Cttp_Related_IDs]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Cttp_Related_IDs](
	[cttp_id] [varchar](19) NOT NULL,
	[cttp_pt] [nvarchar](1000) NULL,
	[tpp_id] [varchar](19) NULL,
	[tpp_pt] [nvarchar](1000) NULL,
	[tpuu_id] [varchar](19) NULL,
	[tpuu_pt] [nvarchar](1000) NULL,
	[tp_id] [varchar](19) NULL,
	[tp_pt] [nvarchar](1000) NULL,
	[mpp_id] [varchar](19) NULL,
	[mpp_pt] [nvarchar](1000) NULL,
	[mpuu_id] [varchar](19) NULL,
	[mpuu_pt] [nvarchar](1000) NULL,
	[mp_id] [varchar](19) NULL,
	[mp_pt] [nvarchar](1000) NULL,
 CONSTRAINT [PK_Ctpp_Related_IDs] PRIMARY KEY CLUSTERED 
(
	[cttp_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetCttp_Related_IDs_ByCode]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- Author:		Peter Jordan
-- Create date: Oct 13, 2016
-- Description:	Returns entry from Cttp_Related_IDs summary table

CREATE FUNCTION [dbo].[GetCttp_Related_IDs_ByCode] 
(	
	@Code varchar(19)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT * FROM [Cttp_Related_IDs]
	WHERE 
	--cttp_id = @Code OR tpp_id = @Code OR tpuu_id = @Code OR tp_id = @Code OR mpp_id = @Code OR mpuu_id = @Code OR mp_id = @Code
	@Code IN(cttp_id,tpp_id,tpuu_id,tp_id,mpp_id,mpuu_id,mp_id)
)




GO
/****** Object:  Table [dbo].[nzmt_snomed_product]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[nzmt_snomed_product](
	[nzmt_mp_sctid] [varchar](20) NULL,
	[nzmt_mp_term] [nvarchar](255) NULL,
	[snomed_product_sctid] [varchar](20) NULL,
	[snomed_product_term] [nvarchar](255) NULL
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetMedicinalProductSctMapping]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




-- Author:		Peter Jordan
-- Create date: Oct 21, 2016
-- Description:	SNAPP Mapping From NZMT MP to SCT Product/Substance

CREATE FUNCTION [dbo].[GetMedicinalProductSctMapping] 
(	
	@Code varchar(19)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT [nzmt_mp_sctid]
      ,[nzmt_mp_term]
      ,[snomed_product_sctid]
      ,[snomed_product_term]
	  FROM [NZULM].[dbo].[nzmt_snomed_product]
	  WHERE [nzmt_mp_sctid] = @Code OR ISNULL(@Code,'') = ''
 )


GO
/****** Object:  Table [dbo].[MedicinalProductUnitOfUse]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicinalProductUnitOfUse](
	[MedicinalProductUnitOfUseID] [varchar](19) NOT NULL,
	[PreferredTerm] [nvarchar](1000) NULL,
	[FullySpecifiedname] [nvarchar](1000) NULL,
	[UnitDoseFormIndicatorID] [varchar](19) NULL,
	[UnitDoseFormSizeValue] [float] NULL,
	[UnitDoseFormSizeUnitsID] [varchar](19) NULL,
	[UnitDoseType] [varchar](19) NULL,
	[ManufacturedDoseFormID] [varchar](19) NULL,
	[MedicinalProductID] [varchar](19) NULL,
	[is_retired] [smallint] NULL,
	[prescribe_by_brand] [smallint] NULL,
 CONSTRAINT [PK_MedicinalProductUnitOfUse] PRIMARY KEY CLUSTERED 
(
	[MedicinalProductUnitOfUseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetMedicinalProductUnitOfUse_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Author:		Peter Jordan
-- Create date: Nov 21, 2011
-- Description:	Returns Medicinal Product Units of Use with passed Term

CREATE FUNCTION [dbo].[GetMedicinalProductUnitOfUse_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
	MPUU.MedicinalProductUnitOfUseID as [Unit of Use Code],
	MPUU.PreferredTerm as [Preferred Term]
	--MPUU.FullySpecifiedname as [Fully Specified Name],
	--MD.PreferredTerm as [Medicinal Product],
	--DF.PreferredTerm as [Dose Form],
	--MPUU.UnitDoseFormSizeValue as [Dose Form Size], 
	--UOM.PreferredTerm as [Unit Of Measurement]
	FROM [MedicinalProductUnitOfUse] MPUU 
	--LEFT OUTER JOIN [UnitOfMeasurement] UOM ON UOM.UnitOfMeasurementID = MPUU.UnitDoseFormSizeUnitsID
	--LEFT OUTER JOIN [DoseForm] DF ON DF.DoseFormID = MPUU.ManufacturedDoseFormID
	--LEFT OUTER JOIN [MedicinalProduct] MD ON MD.MedicinalProductID = MPUU.MedicinalProductID
	WHERE MPUU.preferredterm LIKE '%' + @Term + '%'
	AND MPUU.is_retired = 0
)

GO
/****** Object:  Table [dbo].[MedicinalProductPack]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicinalProductPack](
	[MedicinalProductPackID] [varchar](19) NOT NULL,
	[PreferredTerm] [nvarchar](1000) NULL,
	[FullySpecifiedname] [nvarchar](1000) NULL,
	[TotalUnitOfUseQuantityValue] [float] NULL,
	[TotalUnitOfUseQuantityUnitsID] [varchar](19) NULL,
	[TotalUnitOfUseSizeValue] [float] NULL,
	[TotalUnitOfUseSizeUnitsID] [varchar](19) NULL,
	[is_retired] [smallint] NULL,
	[prescribe_by_brand] [smallint] NULL,
 CONSTRAINT [PK_MedicinalProductPack] PRIMARY KEY CLUSTERED 
(
	[MedicinalProductPackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetMedicinalProductPack_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Author:		Peter Jordan
-- Create date: Nov 22, 2011
-- Description:	Returns Medicinal Product Packs with passed Term

CREATE FUNCTION [dbo].[GetMedicinalProductPack_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
	MPP.MedicinalProductPackID as [Pack Code],
	MPP.PreferredTerm as [Preferred Term]
	--MPP.FullySpecifiedname as [Fully Specified Name],
	--MPP.TotalUnitOfUseQuantityValue as [Quantity],
	--UOM.PreferredTerm as [Quantity Units],
	--MPP.TotalUnitOfUseSizeValue [Size],
	--UOM2.PreferredTerm as [Size Units]
	FROM [MedicinalProductPack] MPP
	--LEFT OUTER JOIN [UnitOfMeasurement] UOM ON UOM.UnitOfMeasurementID = MPP.TotalUnitOfUseQuantityUnitsID
	--LEFT OUTER JOIN [UnitOfMeasurement] UOM2 ON UOM.UnitOfMeasurementID = MPP.TotalUnitOfUseSizeUnitsID
	WHERE MPP.preferredterm LIKE '%' + @Term + '%'
	AND MPP.is_retired = 0
)


GO
/****** Object:  Table [dbo].[TradeProductPack]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TradeProductPack](
	[TradeProductPackID] [varchar](19) NOT NULL,
	[PreferredTerm] [nvarchar](1000) NULL,
	[FullySpecifiedname] [nvarchar](1000) NULL,
	[TradeProductID] [varchar](19) NULL,
	[MedicinalProductPackID] [varchar](19) NOT NULL,
	[OtherPackInformation] [nvarchar](255) NULL,
	[TotalUnitOfUseQuantityValue] [float] NULL,
	[TotalUnitOfUseQuantityUnitsID] [varchar](19) NULL,
	[TotalUnitOfUseSizeValue] [float] NULL,
	[TotalUnitOfUseSizeUnitsID] [varchar](19) NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_TradeProductPack] PRIMARY KEY CLUSTERED 
(
	[TradeProductPackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetTradeProductPack_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Author:		Peter Jordan
-- Create date: Nov 22, 2011
-- Description:	Returns Trade Product Packs with passed Term

CREATE FUNCTION [dbo].[GetTradeProductPack_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
	TPP.TradeProductPackID as [Product Pack Code],
	TPP.PreferredTerm as [Preferred Term]
	--TPP.FullySpecifiedname as [Fully Specified Name],
	--TP.PreferredTerm as [Trade Product],
	--MPP.PreferredTerm as [Medicinal Product Pack],
	--TPP.TotalUnitOfUseQuantityValue as [Quantity],
	--UOM.PreferredTerm as [Quantity Units],
	--TPP.TotalUnitOfUseSizeValue [Size],
	--UOM2.PreferredTerm as [Size Units]
	FROM [TradeProductPack] TPP
	--LEFT OUTER JOIN [TradeProduct] TP ON TP.TradeProductID = TPP.TradeProductID
	--LEFT OUTER JOIN [MedicinalProductPack] MPP ON MPP.MedicinalProductPackID = TPP.MedicinalProductPackID
	--LEFT OUTER JOIN [UnitOfMeasurement] UOM ON UOM.UnitOfMeasurementID = TPP.TotalUnitOfUseQuantityUnitsID
	--LEFT OUTER JOIN [UnitOfMeasurement] UOM2 ON UOM.UnitOfMeasurementID = TPP.TotalUnitOfUseSizeUnitsID
	WHERE TPP.PreferredTerm LIKE '%' + @Term + '%'
	AND TPP.is_retired = 0
)


GO
/****** Object:  Table [dbo].[TradeProduct]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TradeProduct](
	[TradeProductID] [varchar](19) NOT NULL,
	[PreferredTerm] [nvarchar](1000) NULL,
	[FullySpecifiedname] [nvarchar](1000) NULL,
	[SponsorID] [varchar](19) NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_TradeProduct] PRIMARY KEY CLUSTERED 
(
	[TradeProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[TradeProductUnitOfUse]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TradeProductUnitOfUse](
	[TradeProductUnitOfUseID] [varchar](19) NOT NULL,
	[PreferredTerm] [nvarchar](1000) NULL,
	[FullySpecifiedname] [nvarchar](1000) NULL,
	[OtherIdentifyingInformation] [nvarchar](255) NULL,
	[ManufacturedDoseFormID] [varchar](19) NULL,
	[ProprietaryDoseFormID] [varchar](19) NULL,
	[MedicinalProductUnitOfUseID] [varchar](19) NULL,
	[TradeProductID] [varchar](19) NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_TradeProductUnitOfUse] PRIMARY KEY CLUSTERED 
(
	[TradeProductUnitOfUseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DoseForm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DoseForm](
	[DoseFormID] [varchar](19) NOT NULL,
	[FullySpecifiedName] [nvarchar](255) NULL,
	[PreferredTerm] [nvarchar](255) NULL,
	[parentDoseFormID] [varchar](19) NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_DoseForm] PRIMARY KEY CLUSTERED 
(
	[DoseFormID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetTradeProductUnitOfUse_ByMpuuCode]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Author:		Peter Jordan
-- Create date: Nov 21, 2011
-- Description:	Returns Trade Product Units of Use with passed MPUU Code

CREATE FUNCTION [dbo].[GetTradeProductUnitOfUse_ByMpuuCode] 
(	
	@Code varchar(19)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
	TPUU.TradeProductUnitOfUseID as [Unit of Use Code],
	TPUU.PreferredTerm as [Preferred Term],
	--TPUU.FullySpecifiedName as [Fully Specified Name],
	TPUU.OtherIdentifyingInformation as [Other Indentifying Info],
	TD.PreferredTerm as [Trade Product],
	DF.PreferredTerm as [Manuf Dose Form],
	DF2.PreferredTerm as [Prop Dose Form]
	FROM [TradeProductUnitOfUse] TPUU 
	LEFT OUTER JOIN [DoseForm] DF ON DF.DoseFormID = TPUU.ManufacturedDoseFormID
	LEFT OUTER JOIN [DoseForm] DF2 ON DF2.DoseFormID = TPUU.ProprietaryDoseFormID
	LEFT OUTER JOIN [TradeProduct] TD ON TD.TradeProductID = TPUU.TradeProductID
	WHERE TPUU.MedicinalProductUnitOfUseID = @Code
	AND TPUU.is_retired = 0
)

GO
/****** Object:  Table [dbo].[ContaineredTradeProductPack]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContaineredTradeProductPack](
	[ContaineredTradeProductPackID] [varchar](19) NOT NULL,
	[PreferredTerm] [nvarchar](1000) NULL,
	[FullySpecifiedname] [nvarchar](1000) NULL,
	[TradeProductPackID] [varchar](19) NOT NULL,
	[OtherContaineredPackInformation] [nvarchar](255) NULL,
	[PackManufactureIndicatorID] [varchar](19) NULL,
	[ContainerTypeID] [varchar](19) NULL,
	[Pharmacode] [int] NULL,
	[is_section29] [bit] NOT NULL,
	[is_virtual] [bit] NOT NULL,
	[old_sctID] [varchar](19) NULL,
	[Is_Retired] [smallint] NULL,
	[not_in_Medsafe_data] [smallint] NULL,
 CONSTRAINT [PK_ContaineredTradeProductPack] PRIMARY KEY CLUSTERED 
(
	[ContaineredTradeProductPackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetContaineredTradeProductPack_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- Author:		Peter Jordan
-- Create date: Nov 24, 2011
-- Description:	Returns Containered Trade Product Packs with passed Term

CREATE FUNCTION [dbo].[GetContaineredTradeProductPack_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
	CTPP.TradeProductPackID as [Containered Pack Code],
	CTPP.PreferredTerm as [Preferred Term]
	--CTPP.FullySpecifiedname as [Fully Specified Name],
	--CTPP.OtherContaineredPackInformation as [Other Info],
	--TPP.PreferredTerm as [Trade Product Pack],
	--MPP.PreferredTerm as [Medicinal Product Pack],
	--TPP.TotalUnitOfUseQuantityValue as [Quantity],
	--UOM.PreferredTerm as [Quantity Units],
	--TPP.TotalUnitOfUseSizeValue [Size],
	--UOM2.PreferredTerm as [Size Units],
	--CTPP.Pharmacode as [Pharmacode]
	FROM [ContaineredTradeProductPack] CTPP
	--LEFT OUTER JOIN [TradeProductPack] TPP ON TPP.TradeProductPackID = CTPP.TradeProductPackID
	--LEFT OUTER JOIN [MedicinalProductPack] MPP ON MPP.MedicinalProductPackID = TPP.MedicinalProductPackID
	--LEFT OUTER JOIN [UnitOfMeasurement] UOM ON UOM.UnitOfMeasurementID = TPP.TotalUnitOfUseQuantityUnitsID
	--LEFT OUTER JOIN [UnitOfMeasurement] UOM2 ON UOM.UnitOfMeasurementID = TPP.TotalUnitOfUseSizeUnitsID
	WHERE CTPP.PreferredTerm LIKE '%' + @Term + '%'
	AND CTPP.Is_Retired = 0
)



GO
/****** Object:  UserDefinedFunction [dbo].[GetTradeProductUnitOfUse_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Author:		Peter Jordan
-- Create date: Nov 24, 2011
-- Description:	Returns Trade Product Units of Use with passed Term

CREATE FUNCTION [dbo].[GetTradeProductUnitOfUse_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
	TPUU.TradeProductUnitOfUseID as [Unit of Use Code],
	TPUU.PreferredTerm as [Preferred Term]
	--TPUU.FullySpecifiedName as [Fully Specified Name],
	--TP.PreferredTerm as [Trade Product],
	--MPUU.PreferredTerm as [Medicinal Product Unit of Use],
	--DF.PreferredTerm as [Manuf Dose Form],
	--DF2.PreferredTerm as [Prop Dose Form]
	FROM [TradeProductUnitOfUse] TPUU 
	--LEFT OUTER JOIN [DoseForm] DF ON DF.DoseFormID = TPUU.ManufacturedDoseFormID
	--LEFT OUTER JOIN [DoseForm] DF2 ON DF2.DoseFormID = TPUU.ProprietaryDoseFormID
	--LEFT OUTER JOIN [TradeProduct] TP ON TP.TradeProductID = TPUU.TradeProductID
	--LEFT OUTER JOIN [MedicinalProductUnitOfUse] MPUU on MPUU.MedicinalProductUnitOfUseID = TPUU.MedicinalProductUnitOfUseID
	WHERE TPUU.PreferredTerm LIKE '%' + @Term + '%'
	AND TPUU.is_retired = 0
)


GO
/****** Object:  UserDefinedFunction [dbo].[GetMedicinalProduct_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Author:		Peter Jordan
-- Create date: Dec 05, 2011
-- Description:	Returns Medicinal Products with passed Term

CREATE FUNCTION [dbo].[GetMedicinalProduct_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
	MP.MedicinalProductID as [Product Code],
	MP.PreferredTerm as [Preferred Term],
	--MP.FullySpecifiedname as [Fully Specified Name],
	MP.IsBaseSubstance as [Base Substance?]
	FROM [MedicinalProduct] MP 
	WHERE MP.preferredterm LIKE '%' + @Term + '%'
	AND MP.is_retired = 0
)


GO
/****** Object:  UserDefinedFunction [dbo].[GetTradeProduct_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Author:		Peter Jordan
-- Create date: Dec 05, 2011
-- Description:	Returns Trade Products with passed Term

CREATE FUNCTION [dbo].[GetTradeProduct_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT 
	TP.TradeProductID as [Product Code],
	TP.PreferredTerm as [Preferred Term]
	--,TP.FullySpecifiedname as [Fully Specified Name]
	FROM [TradeProduct] TP 
	WHERE TP.preferredterm LIKE '%' + @Term + '%'
	AND TP.is_retired = 0
)


GO
/****** Object:  Table [dbo].[MedicinalProductPack_UnitofUse]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicinalProductPack_UnitofUse](
	[MedicinalProductPack_UnitofUseID] [varchar](19) NOT NULL,
	[MedicinalProductPackID] [varchar](19) NULL,
	[MedicinalProductUnitOfUseID] [varchar](19) NULL,
	[UnitOfUseQuantityValue] [float] NULL,
	[UnitOfUseQuantityUnitsID] [varchar](19) NULL,
	[UnitOfUseSizeValue] [float] NULL,
	[UnitOfUseSizeUnitsID] [varchar](19) NULL,
	[PreferredComponentOrder] [float] NULL,
 CONSTRAINT [PK_MedicinalProductPack_UnitofUse] PRIMARY KEY CLUSTERED 
(
	[MedicinalProductPack_UnitofUseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetMedicinalProductsCombined_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- Author:		Peter Jordan
-- Create date: Dec 21, 2011
-- Description:	Returns Medicinal Products Combined Data set with passed Term

CREATE FUNCTION [dbo].[GetMedicinalProductsCombined_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
	MPUU.MedicinalProductUnitOfUseID as [MPUU],
	MP.PreferredTerm as [Product],
	--DSC.Term as [Product Synonym],
	--DT.[type_name] as [Desc Type],
	MPUU.PreferredTerm as [Unit of Use],
	MPP.PreferredTerm as [Pack]
	FROM	[MedicinalProductUnitOfUse] MPUU 
	LEFT OUTER JOIN [MedicinalProduct] MP ON MP.MedicinalProductID = MPUU.MedicinalProductID
	LEFT OUTER JOIN [MedicinalProductPack_UnitOfUse] MHM ON MHM.MedicinalProductUnitOfUseID = MPUU.MedicinalProductUnitOfUseID
	LEFT OUTER JOIN [MedicinalProductPack] MPP ON MPP.MedicinalProductPackID = MHM.MedicinalProductPackID
	--LEFT OUTER JOIN [Synonyms] SYN ON SYN.primary_sctID = MP.MedicinalProductID
	--LEFT OUTER JOIN [Descriptions] DSC ON DSC.parent_ID = SYN.synonym_sctID
	--LEFT OUTER JOIN [DescriptionType] DT ON DT.DescriptionTypeID = DSC.description_type_id
	WHERE MPUU.preferredterm LIKE '%' + @Term + '%'
	AND MPUU.is_retired = 0
	--AND ISNULL(DT.[short_code],'X') = 'PT'
	--AND ISNULL(DSC.Term,'') <> MP.PreferredTerm
)


GO
/****** Object:  Table [dbo].[TradeProductPack_UnitofUse]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TradeProductPack_UnitofUse](
	[TradeProductPack_UnitofUseID] [varchar](19) NOT NULL,
	[TradeProductPackID] [varchar](19) NULL,
	[TradeProductUnitOfUseID] [varchar](19) NULL,
	[UnitOfUseQuantityValue] [float] NULL,
	[UnitOfUseQuantityUnitsID] [varchar](19) NULL,
	[UnitOfUseSizeValue] [float] NULL,
	[UnitOfUseSizeUnitsID] [varchar](19) NULL,
 CONSTRAINT [PK_TradeProductPack_UnitofUse] PRIMARY KEY CLUSTERED 
(
	[TradeProductPack_UnitofUseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Descriptions]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Descriptions](
	[DescriptionID] [varchar](19) NOT NULL,
	[is_retired] [smallint] NULL,
	[parent_ID] [varchar](19) NOT NULL,
	[parent_table] [varchar](30) NOT NULL,
	[term] [nvarchar](1000) NULL,
	[description_type_id] [varchar](19) NOT NULL,
	[valid_from] [varchar](20) NULL,
	[valid_to] [varchar](20) NULL,
 CONSTRAINT [PK_Descriptions] PRIMARY KEY CLUSTERED 
(
	[DescriptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[DescriptionType]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DescriptionType](
	[DescriptionTypeID] [varchar](19) NOT NULL,
	[type_name] [varchar](25) NULL,
	[short_code] [varchar](8) NULL,
	[max_length] [int] NULL,
	[description] [varchar](150) NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_DescriptionType] PRIMARY KEY CLUSTERED 
(
	[DescriptionTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetTradeProductsCombined_ByTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Author:		Peter Jordan
-- Create date: Dec 22, 2011
-- Description:	Returns Trade Products Combined Data set with passed Term

CREATE FUNCTION [dbo].[GetTradeProductsCombined_ByTerm] 
(	
	@Term varchar(max)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT DISTINCT
	TP.PreferredTerm as [Product],
	TPUU.PreferredTerm as [Unit of Use],
	ISNULL(D.Term,'') as [PrescribingTerm],
	TPP.PreferredTerm as [Pack],
	CTPP.PreferredTerm as [Containered Pack],
	MPP.PreferredTerm as [Medicinal Product Pack],
	MPPUU.MedicinalProductUnitOfUseID as [MPUU]
	FROM	[TradeProductUnitOfUse] TPUU 
	LEFT OUTER JOIN [Descriptions] D ON D.parent_ID = TPUU.TradeProductUnitOfUseID
	LEFT OUTER JOIN [DescriptionType] DT ON DT.DescriptionTypeID = D.description_type_id
	LEFT OUTER JOIN [TradeProduct] TP ON TP.TradeProductID = TPUU.TradeProductID
	LEFT OUTER JOIN [TradeProductPack_UnitOfUse] THT ON THT.TradeProductUnitOfUseID = TPUU.TradeProductUnitOfUseID
	LEFT OUTER JOIN [TradeProductPack] TPP ON TPP.TradeProductPackID = THT.TradeProductPackID
	LEFT OUTER JOIN [ContaineredTradeProductPack] CTPP ON CTPP.TradeProductPackID = TPP.TradeProductPackID
	LEFT OUTER JOIN [MedicinalProductPack] MPP ON MPP.MedicinalProductPackID = TPP.MedicinalProductPackID
	LEFT OUTER JOIN [MedicinalProductPack_UnitofUse] MPPUU ON MPPUU.MedicinalProductPackID = MPP.MedicinalProductPackID
	WHERE (TPUU.preferredterm LIKE '%' + @Term + '%' OR ISNULL(D.Term,'') LIKE '%' + @Term + '%')
	AND TPUU.is_retired = 0 
	AND ISNULL(D.is_retired,-1) = 0
	AND ISNULL(DT.short_code,'XX') = 'PRES'
)



GO
/****** Object:  Table [dbo].[UnitOfMeasurement]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitOfMeasurement](
	[UnitOfMeasurementID] [varchar](19) NOT NULL,
	[FullySpecifiedName] [nvarchar](255) NULL,
	[PreferredTerm] [nvarchar](255) NULL,
	[parentUnitOfMeasurementID] [varchar](19) NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_UnitOfMeasurement] PRIMARY KEY CLUSTERED 
(
	[UnitOfMeasurementID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PreferredStrengthRepresentationType]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreferredStrengthRepresentationType](
	[PreferredStrengthRepresentationTypeID] [varchar](19) NOT NULL,
	[FullySpecifiedName] [nvarchar](255) NULL,
	[PreferredTerm] [nvarchar](255) NULL,
	[is_retired] [smallint] NULL,
	[typeCode] [varchar](5) NULL,
 CONSTRAINT [PK_PreferredStrengthRepresentationType] PRIMARY KEY CLUSTERED 
(
	[PreferredStrengthRepresentationTypeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  UserDefinedFunction [dbo].[GetIngredients_ByMpuuCode]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- Author:		Peter Jordan
-- Create date: Jan 4, 2012
-- Description:	Active Ingredients of passed MPUU code

CREATE FUNCTION [dbo].[GetIngredients_ByMpuuCode] 
(	
	@Code varchar(19)
)
RETURNS TABLE 
AS
RETURN 
(
	SELECT	S.PreferredTerm as [Active Ingredient],
	        [Strength] = dbo.GetIngredientStrength(PSRT.typeCode
									   ,MPUUSAI.BaseFormStrengthNumeratorValue
									   ,U1.PreferredTerm
									   ,MPUUSAI.BaseFormStrengthDenominatorValue
									   ,U2.PreferredTerm
									   ,MPUUSAI.BaseFormStrengthOtherRepresentation
									   ,MPUUSAI.SaltFormStrengthNumeratorValue
									   ,U3.PreferredTerm
									   ,MPUUSAI.SaltFormStrengthDenominatorValue
									   ,U4.PreferredTerm
									   ,MPUUSAI.SaltFormStrengthOtherRepresentation),
			S2.PreferredTerm as [Less Modified Ingredient] 
			FROM [MedicinalProductUnitOfUse_SpecialActiveIngredient] MPUUSAI
		    LEFT OUTER JOIN [PreferredStrengthRepresentationType] PSRT ON PSRT.PreferredStrengthRepresentationTypeID = MPUUSAI.PreferredStrengthRepresentationTypeID
			LEFT OUTER JOIN [Substance] S ON S.SubstanceID = ISNULL(NULLIF(MPUUSAI.Base_SubstanceID,''),MPUUSAI.Salt_SubstanceID)
			LEFT OUTER JOIN [Substance] S2 ON S2.SubstanceID = S.HasLessModifiedIngredientID
			LEFT OUTER JOIN [UnitOfMeasurement] U1 ON U1.UnitOfMeasurementID = MPUUSAI.BaseFormStrengthNumeratorUnitsID
			LEFT OUTER JOIN [UnitOfMeasurement] U2 ON U2.UnitOfMeasurementID = MPUUSAI.BaseFormStrengthDenominatorUnitsID
			LEFT OUTER JOIN [UnitOfMeasurement] U3 ON U3.UnitOfMeasurementID = MPUUSAI.SaltFormStrengthNumeratorUnitsID
			LEFT OUTER JOIN [UnitOfMeasurement] U4 ON U4.UnitOfMeasurementID = MPUUSAI.SaltFormStrengthDenominatorUnitsID
			WHERE MPUUSAI.MedicinalProductUnitOfUseID = @Code
				AND ISNULL(S.is_retired,0) = 0 
				AND ISNULL(S2.is_retired,0) = 0
)



GO
/****** Object:  Table [dbo].[Classification]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Classification](
	[ClassificationID] [varchar](19) NOT NULL,
	[DatasetID] [varchar](19) NULL,
	[SubstanceID] [varchar](19) NULL,
	[SubstanceName] [nvarchar](255) NULL,
	[Classification] [nvarchar](50) NULL,
	[Conditions] [nvarchar](max) NULL,
 CONSTRAINT [PK_Classification] PRIMARY KEY CLUSTERED 
(
	[ClassificationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[PreferredTermIndex]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PreferredTermIndex](
	[PreferredTermIndexID] [varchar](19) NOT NULL,
	[PreferredTerm] [nvarchar](1000) NULL,
	[search_text] [nvarchar](1000) NULL,
	[Pharmacode] [int] NULL,
	[table_name] [varchar](70) NULL,
	[has_subsidy] [smallint] NULL,
	[synonym_for] [varchar](19) NULL,
	[parentMP] [varchar](19) NULL,
	[num_children] [int] NULL,
 CONSTRAINT [PK_PreferredTermIndex] PRIMARY KEY CLUSTERED 
(
	[PreferredTermIndexID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[UnitDoseFormIndicator]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UnitDoseFormIndicator](
	[UnitDoseFormIndicatorID] [varchar](19) NOT NULL,
	[FullySpecifiedName] [nvarchar](255) NULL,
	[PreferredTerm] [nvarchar](255) NULL,
	[is_retired] [smallint] NULL,
 CONSTRAINT [PK_UnitDoseFormIndicator] PRIMARY KEY CLUSTERED 
(
	[UnitDoseFormIndicatorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Index [IX_Pharmacode]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_Pharmacode] ON [dbo].[ContaineredTradeProductPack]
(
	[Pharmacode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TradeProductPackID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_TradeProductPackID] ON [dbo].[ContaineredTradeProductPack]
(
	[TradeProductPackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_parentDoseFormID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_parentDoseFormID] ON [dbo].[DoseForm]
(
	[parentDoseFormID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MP_Has_Substance_MedicinalProductID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_MP_Has_Substance_MedicinalProductID] ON [dbo].[MedicinalProduct_Has_Substance]
(
	[MedicinalProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MedicinalProductPackID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_MedicinalProductPackID] ON [dbo].[MedicinalProductPack_UnitofUse]
(
	[MedicinalProductPackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MedicinalProductUnitOfUseID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_MedicinalProductUnitOfUseID] ON [dbo].[MedicinalProductPack_UnitofUse]
(
	[MedicinalProductUnitOfUseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MedicinalProductID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_MedicinalProductID] ON [dbo].[MedicinalProductUnitOfUse]
(
	[MedicinalProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_Prescribing_Term_Selection_List_PT]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_Prescribing_Term_Selection_List_PT] ON [dbo].[Prescribing_Term_Selection_List]
(
	[prescribing_term] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_HasLessModifiedIngredientID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_HasLessModifiedIngredientID] ON [dbo].[Substance]
(
	[HasLessModifiedIngredientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_PreferredTerm]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_PreferredTerm] ON [dbo].[Substance]
(
	[PreferredTerm] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MedicinalProductPackID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_MedicinalProductPackID] ON [dbo].[TradeProductPack]
(
	[MedicinalProductPackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TradeProductID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_TradeProductID] ON [dbo].[TradeProductPack]
(
	[TradeProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TradeProductPackID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_TradeProductPackID] ON [dbo].[TradeProductPack_UnitofUse]
(
	[TradeProductPackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TradeProductUnitOfUseID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_TradeProductUnitOfUseID] ON [dbo].[TradeProductPack_UnitofUse]
(
	[TradeProductUnitOfUseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_MedicinalProductUnitOfUse]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_MedicinalProductUnitOfUse] ON [dbo].[TradeProductUnitOfUse]
(
	[MedicinalProductUnitOfUseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_TradeProductID]    Script Date: 24/11/2017 4:12:16 PM ******/
CREATE NONCLUSTERED INDEX [IX_TradeProductID] ON [dbo].[TradeProductUnitOfUse]
(
	[TradeProductID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT ('') FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT (NULL) FOR [FullySpecifiedname]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT (NULL) FOR [OtherContaineredPackInformation]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT (NULL) FOR [PackManufactureIndicatorID]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT (NULL) FOR [ContainerTypeID]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT (NULL) FOR [Pharmacode]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT ((0)) FOR [is_section29]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT ((0)) FOR [is_virtual]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT ('') FOR [old_sctID]
GO
ALTER TABLE [dbo].[ContaineredTradeProductPack] ADD  DEFAULT ('0') FOR [Is_Retired]
GO
ALTER TABLE [dbo].[DoseForm] ADD  DEFAULT (NULL) FOR [FullySpecifiedName]
GO
ALTER TABLE [dbo].[DoseForm] ADD  DEFAULT (NULL) FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[DoseForm] ADD  DEFAULT (NULL) FOR [parentDoseFormID]
GO
ALTER TABLE [dbo].[DoseForm] ADD  DEFAULT ('0') FOR [is_retired]
GO
ALTER TABLE [dbo].[MedicinalProduct] ADD  DEFAULT ('') FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[MedicinalProduct] ADD  DEFAULT (NULL) FOR [FullySpecifiedname]
GO
ALTER TABLE [dbo].[MedicinalProduct] ADD  DEFAULT ((0)) FOR [IsBaseSubstance]
GO
ALTER TABLE [dbo].[MedicinalProduct] ADD  DEFAULT ((0)) FOR [is_retired]
GO
ALTER TABLE [dbo].[MedicinalProductPack] ADD  DEFAULT ('') FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[MedicinalProductPack] ADD  DEFAULT (NULL) FOR [FullySpecifiedname]
GO
ALTER TABLE [dbo].[MedicinalProductPack] ADD  DEFAULT (NULL) FOR [TotalUnitOfUseQuantityValue]
GO
ALTER TABLE [dbo].[MedicinalProductPack] ADD  DEFAULT (NULL) FOR [TotalUnitOfUseQuantityUnitsID]
GO
ALTER TABLE [dbo].[MedicinalProductPack] ADD  DEFAULT (NULL) FOR [TotalUnitOfUseSizeValue]
GO
ALTER TABLE [dbo].[MedicinalProductPack] ADD  DEFAULT (NULL) FOR [TotalUnitOfUseSizeUnitsID]
GO
ALTER TABLE [dbo].[MedicinalProductPack] ADD  DEFAULT ('0') FOR [is_retired]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT ('') FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT (NULL) FOR [FullySpecifiedname]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT (NULL) FOR [UnitDoseFormIndicatorID]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT ('0') FOR [UnitDoseFormSizeValue]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT (NULL) FOR [UnitDoseFormSizeUnitsID]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT (NULL) FOR [UnitDoseType]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT (NULL) FOR [ManufacturedDoseFormID]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT (NULL) FOR [MedicinalProductID]
GO
ALTER TABLE [dbo].[MedicinalProductUnitOfUse] ADD  DEFAULT ('0') FOR [is_retired]
GO
ALTER TABLE [dbo].[TradeProduct] ADD  DEFAULT ('') FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[TradeProduct] ADD  DEFAULT (NULL) FOR [FullySpecifiedname]
GO
ALTER TABLE [dbo].[TradeProduct] ADD  DEFAULT (NULL) FOR [SponsorID]
GO
ALTER TABLE [dbo].[TradeProduct] ADD  DEFAULT ((0)) FOR [is_retired]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT ('') FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT (NULL) FOR [FullySpecifiedname]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT (NULL) FOR [TradeProductID]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT (NULL) FOR [OtherPackInformation]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT (NULL) FOR [TotalUnitOfUseQuantityValue]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT (NULL) FOR [TotalUnitOfUseQuantityUnitsID]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT (NULL) FOR [TotalUnitOfUseSizeValue]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT (NULL) FOR [TotalUnitOfUseSizeUnitsID]
GO
ALTER TABLE [dbo].[TradeProductPack] ADD  DEFAULT ('0') FOR [is_retired]
GO
ALTER TABLE [dbo].[TradeProductUnitOfUse] ADD  DEFAULT ('') FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[TradeProductUnitOfUse] ADD  DEFAULT (NULL) FOR [FullySpecifiedname]
GO
ALTER TABLE [dbo].[TradeProductUnitOfUse] ADD  DEFAULT (NULL) FOR [OtherIdentifyingInformation]
GO
ALTER TABLE [dbo].[TradeProductUnitOfUse] ADD  DEFAULT (NULL) FOR [ManufacturedDoseFormID]
GO
ALTER TABLE [dbo].[TradeProductUnitOfUse] ADD  DEFAULT (NULL) FOR [ProprietaryDoseFormID]
GO
ALTER TABLE [dbo].[TradeProductUnitOfUse] ADD  DEFAULT (NULL) FOR [MedicinalProductUnitOfUseID]
GO
ALTER TABLE [dbo].[TradeProductUnitOfUse] ADD  DEFAULT (NULL) FOR [TradeProductID]
GO
ALTER TABLE [dbo].[TradeProductUnitOfUse] ADD  DEFAULT ('0') FOR [is_retired]
GO
ALTER TABLE [dbo].[UnitDoseFormIndicator] ADD  DEFAULT (NULL) FOR [FullySpecifiedName]
GO
ALTER TABLE [dbo].[UnitDoseFormIndicator] ADD  DEFAULT (NULL) FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[UnitDoseFormIndicator] ADD  DEFAULT ('0') FOR [is_retired]
GO
ALTER TABLE [dbo].[UnitOfMeasurement] ADD  DEFAULT (NULL) FOR [FullySpecifiedName]
GO
ALTER TABLE [dbo].[UnitOfMeasurement] ADD  DEFAULT (NULL) FOR [PreferredTerm]
GO
ALTER TABLE [dbo].[UnitOfMeasurement] ADD  DEFAULT (NULL) FOR [parentUnitOfMeasurementID]
GO
ALTER TABLE [dbo].[UnitOfMeasurement] ADD  DEFAULT ('0') FOR [is_retired]
GO
/****** Object:  StoredProcedure [dbo].[nzmt_snomed_update]    Script Date: 24/11/2017 4:12:16 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






-- ============================================= 
-- Author:      Peter Jordan
-- Create date: 23rd October 2016
-- Description: Update Row in NZMT_SNOMED Product Table
-- ============================================= 
CREATE PROCEDURE [dbo].[nzmt_snomed_update]
(
           @MPID varchar(255),
           @SCTID varchar(20),
           @TERM nvarchar(255)
) 

AS 

UPDATE dbo.nzmt_snomed_product
SET snomed_product_sctid = @SCTID, snomed_product_term = @TERM 
WHERE nzmt_mp_sctid = @MPID

IF @@ROWCOUNT <> 1 RETURN -1

RETURN 0







GO
USE [master]
GO
ALTER DATABASE [NZULM] SET  READ_WRITE 
GO
