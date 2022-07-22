--delete duplicate values 

SELECT  COUNT(m.[Date]) as 'Total Dates', s.ID
FROM [NeeksDB].[dbo].[MinuteData] m INNER JOIN Stock s on m.StockID = s.ID
GROUP BY s.ID
ORDER BY 'Total Dates' desc


SELECT  COUNT(DISTINCT m.[Date]) as 'Total Dates', s.ID
FROM [NeeksDB].[dbo].[MinuteData] m INNER JOIN Stock s on m.StockID = s.ID
GROUP BY s.ID
ORDER BY 'Total Dates' desc

SELECT COUNT(ID)
FROM MinuteData

--7949
SELECT m.ID, m.[Date], m.StockID, m.Average, m.Volume, m.AmtOfTrades
FROM [NeeksDB].[dbo].[MinuteData] m INNER JOIN Stock s on m.StockID = s.ID
WHERE s.ID = 7949
ORDER BY m.[Date] desc

-- duplicate deleting query 
-- idea 1: inner join minute data w minute data on ?

--idea 2: using over, partition, and cte

WITH cte AS (
    SELECT 
	*,
        ROW_NUMBER() OVER (
            PARTITION BY 
                StockID, 
                Date, 
                ID
            ORDER BY 
                StockID, 
                Date, 
                ID
        ) row_num
     FROM 
        dbo.MinuteData
)
Select * 
FROM cte

--DELETE FROM cte
--WHERE row_num > 1;





