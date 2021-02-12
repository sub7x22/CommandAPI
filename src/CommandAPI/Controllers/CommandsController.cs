using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CommandAPI.Data;
using CommandAPI.Models;
using AutoMapper;
using CommandAPI.Dtos;
using Microsoft.AspNetCore.JsonPatch;
using System;

namespace CommandAPI.Controllers
{
    [Route("api/commands")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        //bs random change
        private readonly ICommandAPIRepo _repository;
        private readonly IMapper _mapper;
        public CommandsController(ICommandAPIRepo repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        [HttpGet]
        public ActionResult<IEnumerable<CommandReadDto>> GetAllCommands()
        {
            var commandItems = _repository.GetAllCommands();

            return Ok(_mapper.Map<IEnumerable<CommandReadDto>>(commandItems));
        }

        [HttpGet("{id}", Name="GetCommandByID")]
        public ActionResult<CommandReadDto> GetCommandById(int id)
        {
            var commandItem = _repository.GetCommandById(id);
            if (commandItem == null)
            {
                return NotFound();
            }
            Console.WriteLine($"The ID for '{commandItem.CommandLine}' is: {commandItem.Id}");
            return Ok(_mapper.Map<CommandReadDto>(commandItem));
        }

        [HttpPost]
        public ActionResult <CommandReadDto> CreateCommand(CommandCreateDto commandCreateDto)
            {
                var commandModel = _mapper.Map<Command>(commandCreateDto);
                _repository.CreateCommand(commandModel);
                _repository.SaveChanges();

                var commandReadDto = _mapper.Map<CommandReadDto>(commandModel);

                return CreatedAtRoute("GetCommandByID",
                    new {id = commandReadDto.Id}, commandReadDto);
            }

            [HttpPut("{id}")]
            public ActionResult UpdateCommand(int id, CommandUpdateDto commandUpdateDto)
            {
                var commandModelFromRepo = _repository.GetCommandById(id);
                if (commandModelFromRepo == null)
                {
                    return NotFound();
                }
                _mapper.Map(commandUpdateDto, commandModelFromRepo);
                _repository.UpdateCommand(commandModelFromRepo);
                _repository.SaveChanges();
                return NoContent();
            }

            [HttpPatch("{id}")]
            public ActionResult PartialCommandUpdate(int id, JsonPatchDocument<CommandUpdateDto> patchDoc)
            {
                var commandModelFromRepo = _repository.GetCommandById(id);
                if(commandModelFromRepo == null)
                {
                    return NotFound();
                }

                var commandToPatch = _mapper.Map<CommandUpdateDto>(commandModelFromRepo);
                patchDoc.ApplyTo(commandToPatch, ModelState);

                if(!TryValidateModel(commandToPatch))
                {
                    return ValidationProblem(ModelState);
                }

                _mapper.Map(commandToPatch, commandModelFromRepo);

                _repository.UpdateCommand(commandModelFromRepo);

                _repository.SaveChanges();

                return NoContent();
            }

            [HttpDelete("{id}")]
            public ActionResult DeleteCommand(int id)
            {
                var commandModelFromRepo = _repository.GetCommandById(id);
                if(commandModelFromRepo == null)
                {
                    return NotFound();
                }
                _repository.DeleteCommand(commandModelFromRepo);
                _repository.SaveChanges();

                return NoContent();
            }
    }
}