WITH cte AS (
    SELECT 
	*,
        ROW_NUMBER() OVER (
            PARTITION BY 
                StockID, 
                Date                
            ORDER BY 
                StockID, 
                Date, 
                ID
        ) row_num
     FROM 
        dbo.MinuteData
)
select * from cte
--DELETE FROM cte
WHERE cte.row_num > 1;

--SELECT * 
--FROM MinuteData

--select * into dbo.MinuteDataCopy from dbo.MinuteData  


