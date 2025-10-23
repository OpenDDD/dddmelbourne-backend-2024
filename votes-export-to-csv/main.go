package main

import (
	"context"
	"encoding/csv"
	"encoding/json"
	"fmt"
	"log"
	"os"
	"strconv"

	"github.com/Azure/azure-sdk-for-go/sdk/data/aztables"
)

func main() {
	client, err := aztables.NewServiceClientFromConnectionString("<PASTE CONNECTION STRING HERE>", nil)
	if err != nil {
		log.Fatal(err)
	}

	table := client.NewClient("Submissions")
	err = exportTable(table, []string{"PartitionKey", "RowKey", "ExternalId", "Title", "Session"}, "Submissions.csv")
	if err != nil {
		log.Fatal(err)
	}

	table = client.NewClient("Submitters")
	err = exportTable(table, []string{"PartitionKey", "RowKey", "ExternalId", "Name", "Presenter"}, "Submitters.csv")
	if err != nil {
		log.Fatal(err)
	}

	table = client.NewClient("EloVotes")
	err = exportTable(table, []string{"PartitionKey", "RowKey", "WinnerSessionId", "LoserSessionId", "IsDraw", "IpAddress", "VoterSessionId", "VoterTicket", "VoterLastname"}, "EloVotes.csv")
	if err != nil {
		log.Fatal(err)
	}
}

func exportTable(table *aztables.Client, columns []string, filename string) error {
	file, err := os.Create("./" + filename)
	if err != nil {
		return err
	}
	defer file.Close()

	csvWriter := csv.NewWriter(file)
	err = csvWriter.Write(columns)
	if err != nil {
		return err
	}
	defer csvWriter.Flush()

	pager := table.NewListEntitiesPager(&aztables.ListEntitiesOptions{})
	pageCount := 0
	for pager.More() {
		response, err := pager.NextPage(context.TODO())
		if err != nil {
			panic(err)
		}
		fmt.Printf("There are %d entities in page #%d\n", len(response.Entities), pageCount)
		pageCount += 1

		for _, entity := range response.Entities {
			var myEntity aztables.EDMEntity
			err = json.Unmarshal(entity, &myEntity)
			if err != nil {
				panic(err)
			}

			record := make([]string, len(columns))
			for cidx, column := range columns {
				if column == "PartitionKey" {
					record[cidx] = myEntity.PartitionKey
				} else if column == "RowKey" {
					record[cidx] = myEntity.RowKey
				} else if myEntity.Properties[column] == nil {
					record[cidx] = ""
				} else if b, ok := myEntity.Properties[column].(bool); ok {
					record[cidx] = strconv.FormatBool(b)
				} else {
					record[cidx] = myEntity.Properties[column].(string)
				}
			}
			err = csvWriter.Write(record)
			if err != nil {
				return err
			}
		}
	}

	return nil
}
