﻿using Domain.Dtos.GetDtos;
using Domain.Entities;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Mappers;
using Infrastructure.Persistence;
namespace Infrastructure.Repository
{
    internal sealed class MessageRepository(BackendDbContext _context) : IMessageRepository
    {

        public async Task AddMessageAsync(Message message)
        {
            await _context.Message.AddAsync(message);
            await _context.SaveChangesAsync();
        }


        public async Task<ICollection<GetMessageDto>> GetMessagesAsync(int idUserIssuer, int idUserReceiver) =>
            await _context.Message
                        .Include(m => m.UserIssuer)
                        .Include(m => m.UserReceiver)
                        .Where(m => (m.IdUserIssuer == idUserIssuer && m.Id_UserReceiver == idUserReceiver) ||
                                    (m.IdUserIssuer == idUserReceiver && m.Id_UserReceiver == idUserIssuer))
                        .AsSplitQuery()
                        .Select(m => m.ToGetMessageMapper())
                        .ToListAsync();


        public async Task<bool> DeleteMessageAsync(int id)
        {
            var message = await _context.Message.FirstOrDefaultAsync(m => m.Id_Message == id);

            if (message != null)
            {
                try
                {
                    _context.Message.Remove(message);
                    await _context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateConcurrencyException)
                {
                    _context.Entry(message).Reload();
                    if (message == null)
                    {
                        return true;
                    }
                    return await DeleteMessageAsync(id);
                }
            }

            return false;
        }
    }
}